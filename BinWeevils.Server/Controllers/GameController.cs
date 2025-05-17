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
        private readonly SinglePlayerGamesSettings m_singlePlayerSettings;
        private readonly TurnBasedGamesSettings m_turnBasedSettings;
        private readonly WeevilWheelsSettings m_kartSettings;
        private readonly EconomySettings m_economy;
        
        public GameController(WeevilDBContext dbContext, 
            IOptionsSnapshot<SinglePlayerGamesSettings> singlePlayerSettings, 
            IOptionsSnapshot<TurnBasedGamesSettings> turnBasedSettings, 
            IOptionsSnapshot<WeevilWheelsSettings> kartSettings, 
            IOptionsSnapshot<EconomySettings> economy)
        {
            m_dbContext = dbContext;
            m_singlePlayerSettings = singlePlayerSettings.Value;
            m_turnBasedSettings = turnBasedSettings.Value;
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
                .UpdateIf(x => now-x.m_lastPlayed >= m_singlePlayerSettings.Cooldown)
                .RunAsync();
            return rowsUpserted != 0;
        }
        
        private async Task<bool> UpsertGameAwardGiven(uint weevilIdx, EGameType gameType)
        {
            var rowsUpserted = await m_dbContext.m_weevilGamesPlayed
                .Upsert(new WeevilGamePlayedDB
                {
                    m_gameType = gameType,
                    m_weevilIdx = weevilIdx,
                    m_awardGiven = true // for insert
                })
                .UpdateIf(x => x.m_awardGiven == false)
                .WhenMatched(h => new WeevilGamePlayedDB
                {
                    m_awardGiven = true
                })
                .RunAsync();
            return rowsUpserted != 0;
        }
        
        private async Task<bool> UpsertGame(uint weevilIdx, ETurnBasedGameType gameType)
        {
            var now = DateTime.UtcNow;
            var rowsUpserted = await m_dbContext.m_weevilTurnBasedGamesPlayed
                .Upsert(new WeevilTurnBasedGamePlayedDB
                {
                    m_gameType = gameType,
                    m_lastPlayed = DateTime.UtcNow,
                    m_weevilIdx = weevilIdx
                })
                .UpdateIf(x => now-x.m_lastPlayed >= m_turnBasedSettings.Cooldown)
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
            
            if (!m_singlePlayerSettings.Games.TryGetValue(request.m_gameID, out var gameSettings) || gameSettings.OneTimeAward != null)
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
        
        [StructuredFormPost("has-the-user-played")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<HasTheUserPlayedResponse> HasPlayed([FromBody] HasTheUserPlayedRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.HasPlayed");
            activity?.SetTag("gameID", request.m_gameID);
            
            if (request.m_gameID == EGameType.Invalid)
            {
                // halloween_banquetRoom.swf sends 0 for unknown days...
                // don't let the user play
                return new HasTheUserPlayedResponse
                {
                    m_hasPlayed = 1
                };
            }
            
            if (!m_singlePlayerSettings.Games.TryGetValue(request.m_gameID, out var gameSettings) || gameSettings.OneTimeAward == null)
            {
                throw new InvalidDataException("invalid game for HasPlayed");
            }
            
            var hasPlayed = await m_dbContext.m_weevilGamesPlayed
                .Where(x => x.m_weevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_gameType == request.m_gameID)
                .AnyAsync();
            
            return new HasTheUserPlayedResponse
            {
                m_hasPlayed = hasPlayed ? 1 : 0
            };
        }
        
        [StructuredFormPost("save-game-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SaveGameStatsResponse> SaveGameStats([FromBody] SaveGameStatsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SaveGameStats");
            activity?.SetTag("gameID", request.m_gameID);
            activity?.SetTag("awardGiven", request.m_awardGiven);
            activity?.SetTag("awardedMulch", request.m_awardedMulch);
            
            if (!request.m_awardGiven)
            {
                // ... okay? i wont
                return new SaveGameStatsResponse();
            }
            
            if (!m_singlePlayerSettings.Games.TryGetValue(request.m_gameID, out var gameSettings) || gameSettings.OneTimeAward == null)
            {
                throw new InvalidDataException("invalid game for SaveGameStats");
            }
            
            if (request.m_awardedMulch != 0 && request.m_awardedMulch != gameSettings.OneTimeAward.Mulch)
            {
                throw new InvalidDataException("client specified incorrect awardedMulch");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            if (!await UpsertGameAwardGiven(idx, request.m_gameID))
            {
                throw new Exception("was already awarded");
            }
            
            await m_dbContext.GiveMulchAndXp(idx, 
                gameSettings.OneTimeAward.Mulch ?? 0,
                0);
            await transaction.CommitAsync();
            
            // todo: should probably expand this to give items etc...
            
            return new SaveGameStatsResponse
            {
                m_error = SaveGameStatsResponse.ERROR_SUCCESS
            };
        }
        
        [HttpGet("brain-info")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BrainInfo> GetBrainTrainingInfo()
        {
            using var activity = ApiServerObservability.StartActivity("GameController.GetBrainTrainingInfo");
            
            var deadline = DateTime.UtcNow - m_singlePlayerSettings.Cooldown; 
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
        
        [StructuredFormPost("start")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public StartTurnBasedResponse StartTurnBased([FromBody] StartTurnBasedRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitTimeTrial");
            activity?.SetTag("gameID", request.m_gameID);
            
            if (!Enum.IsDefined(request.m_gameID))
            {
                throw new InvalidDataException("invalid game id");
            }
            
            // what this does doesn't actually matter...
            // the checks will all happen on submit
            return new StartTurnBasedResponse
            {
                m_key = $"{(int)request.m_gameID}"
            };
        }
        
        [StructuredFormPost("submit-turn")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitTurnBasedResponse> SubmitTurnBased([FromBody] SubmitTurnBasedRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitTurnBased");
            activity?.SetTag("authKey", request.m_authKey);
            activity?.SetTag("gameResult", request.m_gameResult);
            
            if (request.m_gameResult != 0 && request.m_gameResult != 1)
            {
                throw new InvalidDataException("invalid game result");
            }
            if (!Enum.TryParse<ETurnBasedGameType>(request.m_authKey, out var gameID))
            {
                throw new InvalidDataException("invalid key");
            }
            if (!m_turnBasedSettings.Games.TryGetValue(gameID, out var game))
            {
                throw new InvalidDataException("invalid game id");
            }
            
            var tag = new KeyValuePair<string, object?>("game", gameID);
            ApiServerObservability.s_gamesPlayedTotal.Add(1, tag);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var idx = await GetIdx();
            if (!await UpsertGame(idx, gameID))
            {
                return new SubmitTurnBasedResponse();
            }
            
            var rewardMulch = m_turnBasedSettings.BaseMulch + request.m_gameResult * m_turnBasedSettings.WinMulch;
            var rewardXp = m_turnBasedSettings.BaseXp + request.m_gameResult * m_turnBasedSettings.WinXp;
            
            await m_dbContext.GiveMulchAndXp(idx, rewardMulch, rewardXp);
            var dto = await m_dbContext.GetMulchAndXp(idx);
            await transaction.CommitAsync();
            
            ApiServerObservability.s_gamesPlayedGivingRewards.Add(1, tag);
            ApiServerObservability.s_gamesMulchRewarded.Add(rewardMulch, tag);
            ApiServerObservability.s_gamesXpRewarded.Add(rewardXp, tag);
            
            return new SubmitTurnBasedResponse
            {
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
            
            ApiServerObservability.s_kartTimeTrialsFinished.Add(1, new KeyValuePair<string, object?>("track", gameID));
            
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
            
            ApiServerObservability.s_kartTrialMulchRewarded.Add(rewardMulch);
            ApiServerObservability.s_kartTrialXpRewarded.Add(rewardXp);
            
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
        public StartTurnBasedResponse StartMultiplayerRace([FromBody] StartMultiplayerRaceRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.StartMultiplayerRace");
            activity?.SetTag("trackID", request.m_trackID);
            
            // what this does doesn't actually matter...
            // the checks will all happen on submit
            return new StartTurnBasedResponse
            {
                m_key = $"{request.m_trackID}"
            };
        }
        
        [StructuredFormPost("submit-race")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitTurnBasedResponse> SubmitMultiplayerRace([FromBody] SubmitMultiplayerRaceRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitMultiplayerRace");
            activity?.SetTag("authKey", request.m_authKey);
            activity?.SetTag("numBeaten", request.m_numBeaten);
            
            if (!m_kartSettings.Enabled)
            {
                throw new Exception("not enabled");
            }
            if (request.m_numBeaten >= 4)
            {
                throw new InvalidDataException("invalid numBeaten");
            }
            if (!byte.TryParse(request.m_authKey, out var trackID))
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
            
            ApiServerObservability.s_kartMultiplayerMulchRewarded.Add(rewardMulch);
            ApiServerObservability.s_kartMultiplayerXpRewarded.Add(rewardXp);
            
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