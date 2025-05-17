using System.Net.Mime;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Form.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/game")]
    public class GameController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly SinglePlayerGamesSettings m_settings;
        private readonly WeevilWheelsSettings m_kartSettings;
        private readonly EconomySettings m_economy;
        
        public GameController(WeevilDBContext dbContext, 
            IOptionsSnapshot<SinglePlayerGamesSettings> settings, 
            IOptionsSnapshot<WeevilWheelsSettings> kartSettings, 
            IOptionsSnapshot<EconomySettings> economy)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
            m_kartSettings = kartSettings.Value;
            m_economy = economy.Value;   
        }
        
        private async Task<uint> GetIdx()
        {
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_idx
                })
                .SingleAsync();
            return dto.m_idx;
        }
        
        private async Task<bool> UpsertGame(uint weevilIdx, EGameType gameType)
        {
            var now = DateTime.UtcNow;
            var rowsUpserted = await m_dbContext.m_weevilGamesPlayed
                .Upsert(new WeevilGamePlayedDB
                {
                    m_gameType = gameType,
                    m_lastPlayed = DateTime.UtcNow,
                    m_weevilIdx = weevilIdx
                })
                .UpdateIf(x => now-x.m_lastPlayed >= m_settings.Cooldown)
                .RunAsync();
            return rowsUpserted != 0;
        }
        
        [StructuredFormPost("submit-single")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitScoreResponse> SubmitSingle([FromBody] SubmitScoreRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitSingle");
            activity?.SetTag("gameID", request.m_gameID);
            activity?.SetTag("score", request.m_score);
            
            if (!m_settings.Games.TryGetValue(request.m_gameID, out var gameSettings) || gameSettings.OneTimeAward != null)
            {
                return new SubmitScoreResponse
                {
                    m_result = SubmitScoreResponse.ERR_WRONG_GAME
                };
            }
            
            var tag = new KeyValuePair<string, object?>("game", request.m_gameID);
            ApiServerObservability.s_gamesPlayedTotal.Add(1, tag);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            if (!await UpsertGame(idx, request.m_gameID))
            {
                return new SubmitScoreResponse
                {
                    m_result = SubmitScoreResponse.ERR_PLAYED_ALREADY
                };
            }
            
            var rewardMulch = (uint)Math.Min(request.m_score * m_economy.GameScoreToMulch, m_economy.MaxMulchPerGame);
            var rewardXp = (uint)Math.Min(request.m_score * m_economy.GameScoreToXp, m_economy.MaxXpPerGame);
            
            await m_dbContext.GiveMulchAndXp(idx, rewardMulch, rewardXp);
            await transaction.CommitAsync();
            
            ApiServerObservability.s_gamesScoreTotal.Add(request.m_score, tag);
            ApiServerObservability.s_gamesPlayedGivingRewards.Add(1, tag);
            ApiServerObservability.s_gamesMulchRewarded.Add(rewardMulch, tag);
            ApiServerObservability.s_gamesXpRewarded.Add(rewardXp, tag);
            
            return new SubmitScoreResponse
            {
                m_result = SubmitScoreResponse.ERR_OK,
                m_mulchEarned = rewardMulch,
                m_xpEarned = rewardXp
            };
        }
        
        [HttpGet("brain-info")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BrainInfo> GetBrainTrainingInfo()
        {
            using var activity = ApiServerObservability.StartActivity("GameController.GetBrainTrainingInfo");
            
            var deadline = DateTime.UtcNow - m_settings.Cooldown; 
            // (efcore cant eval arithmetic on datetime)
            // https://github.com/dotnet/efcore/issues/6025
            var onCooldown = await m_dbContext.m_weevilGamesPlayed
                .Where(x => x.m_weevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_gameType == EGameType.BrainTrain)
                .Where(x => x.m_lastPlayed > deadline)
                .AnyAsync();

            return new BrainInfo
            {
                m_modes = onCooldown ? 1 : 2
            };
        }
        
        [StructuredFormPost("brain-submit")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitDailyBrainResponse> SubmitBrainTrainingScore([FromBody] SubmitDailyBrainRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitBrainTrainingScore");
            activity?.SetTag("score", request.m_score);
            activity?.SetTag("levels", request.m_levels);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            
            var score = Math.Min(request.m_score, m_economy.DailyBrainMaxScore);
            var scoreFac = (float)score/m_economy.DailyBrainMaxScore;
            
            var rewardMulch = (uint)(scoreFac * m_economy.DailyBrainMaxMulch);
            var rewardXp = (uint)(scoreFac * m_economy.DailyBrainMaxXp);
            
            if (!await UpsertGame(idx, EGameType.BrainTrain))
            {
                rewardMulch = 0;
                rewardXp = 0;
            }
            
            await m_dbContext.GiveMulchAndXp(idx, rewardMulch, rewardXp);
            var dto = await m_dbContext.GetMulchAndXp(idx);
            await transaction.CommitAsync();
            
            var tag = new KeyValuePair<string, object?>("game", EGameType.BrainTrain);
            ApiServerObservability.s_gamesPlayedTotal.Add(1, tag);
            if (rewardMulch > 0)
            {
                ApiServerObservability.s_gamesPlayedGivingRewards.Add(1, tag);
                ApiServerObservability.s_gamesMulchRewarded.Add((int)rewardMulch, tag);
                ApiServerObservability.s_gamesXpRewarded.Add((int)rewardXp, tag);
            }
            
            return new SubmitDailyBrainResponse
            {
                m_modes = 1, // we are definitely on cooldown
                m_mulchEarned = rewardMulch,
                m_xpEarned = rewardXp,
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp
            };
        }
        
        [StructuredFormPost("submit-trial")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitTurnBasedResponse> SubmitTimeTrial([FromBody] SubmitTimeTrialRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitTimeTrial");
            activity?.SetTag("trackID", request.m_trackID);
            activity?.SetTag("time", request.m_time);
                        
            if (!m_kartSettings.Enabled)
            {
                throw new Exception("not enabled");
            }
            if (!m_kartSettings.TrackIDToGame.TryGetValue(request.m_trackID, out var gameID))
            {
                throw new InvalidDataException("invalid track id");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            if (!await UpsertGame(idx, gameID))
            {
                return new SubmitTurnBasedResponse();
            }
            
            var track = m_kartSettings.Tracks[gameID];
            var completeTime = TimeSpan.FromMilliseconds(request.m_time);
            
            var interpProgress = track.TrophyTimes[WeevilWheelsTrophyType.Gold] / completeTime;
            interpProgress = Math.Min(interpProgress, 1);
            interpProgress = Math.Max(interpProgress, 0);
            var rewardMulch = (uint)double.Lerp(
                m_kartSettings.MinTimeTrialMulch, 
                m_kartSettings.GoldTimeTrialMulch, 
                interpProgress);
            const uint rewardXp = 0u;
            
            await m_dbContext.GiveMulchAndXp(idx, rewardMulch, rewardXp);
            var dto = await m_dbContext.GetMulchAndXp(idx);
            await transaction.CommitAsync();
            
            return new SubmitTurnBasedResponse
            {
                m_mulchEarned = rewardMulch,
                m_xpEarned = rewardXp,
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp
            };
        }
        
        [StructuredFormPost("start-race")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public StartMultiplayerRaceResponse StartMultiplayerRace([FromBody] StartMultiplayerRaceRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.StartMultiplayerRace");
            activity?.SetTag("trackID", request.m_trackID);
            
            // what this does doesn't actually matter...
            // the checks will all happen on submit
            return new StartMultiplayerRaceResponse
            {
                m_key = $"{request.m_trackID}"
            };
        }
        
        [StructuredFormPost("submit-race")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitTurnBasedResponse> SubmitMultiplayerRace([FromBody] SubmitMultiplayerRaceRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitMultiplayerRace");
            activity?.SetTag("key", request.m_key);
            activity?.SetTag("numBeaten", request.m_numBeaten);
            
            if (!m_kartSettings.Enabled)
            {
                throw new Exception("not enabled");
            }
            if (request.m_numBeaten >= 4)
            {
                throw new InvalidDataException("invalid numBeaten");
            }
            if (!byte.TryParse(request.m_key, out var trackID))
            {
                throw new InvalidDataException("invalid key");
            }
            if (!m_kartSettings.TrackIDToGame.TryGetValue(trackID, out var gameID))
            {
                throw new InvalidDataException("invalid track id");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            if (!await UpsertGame(idx, gameID))
            {
                return new SubmitTurnBasedResponse();
            }
            
            var rewardMulch = m_kartSettings.MultiplayerMulchPerBeaten * request.m_numBeaten + m_kartSettings.MinMultiplayerMulch;
            var rewardXp = m_kartSettings.MultiplayerXpPerBeaten * request.m_numBeaten + m_kartSettings.MinMultiplayerXp;
            
            await m_dbContext.GiveMulchAndXp(idx, rewardMulch, rewardXp);
            var dto = await m_dbContext.GetMulchAndXp(idx);
            await transaction.CommitAsync();
            
            return new SubmitTurnBasedResponse
            {
                m_mulchEarned = rewardMulch,
                m_xpEarned = rewardXp,
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp
            };
        }
        
        // has-the-user-played
        // save-game-stats
    }
}