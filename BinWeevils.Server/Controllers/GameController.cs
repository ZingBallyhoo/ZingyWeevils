using System.Net.Mime;
using BinWeevils.Common;
using BinWeevils.Common.Database;
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
        private readonly EconomySettings m_economy;
        
        public GameController(WeevilDBContext dbContext,  IOptionsSnapshot<SinglePlayerGamesSettings> settings, IOptionsSnapshot<EconomySettings> economy)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
            m_economy = economy.Value;   
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
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_idx
                })
                .SingleAsync();

            var now = DateTime.UtcNow;
            var rowsUpserted = await m_dbContext.m_weevilGamesPlayed
                .Upsert(new WeevilGamePlayedDB
                {
                    m_gameType = request.m_gameID,
                    m_lastPlayed = DateTime.UtcNow,
                    m_weevilIdx = dto.m_idx
                })
                .UpdateIf(x => now-x.m_lastPlayed >= m_settings.Cooldown)
                .RunAsync();
            if (rowsUpserted == 0)
            {
                return new SubmitScoreResponse
                {
                    m_result = SubmitScoreResponse.ERR_PLAYED_ALREADY
                };
            }
            
            var rewardMulch = (uint)Math.Min(request.m_score * m_economy.GameScoreToMulch, m_economy.MaxMulchPerGame);
            var rewardXp = (uint)Math.Min(request.m_score * m_economy.GameScoreToXp, m_economy.MaxXpPerGame);
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == dto.m_idx)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + rewardMulch)
                    .SetProperty(x => x.m_xp, x => x.m_xp + rewardXp)
                );
            await transaction.CommitAsync();
            
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

            return new BrainInfo
            {
                m_modes = 2
            };
        }
        
        [StructuredFormPost("brain-submit")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitDailyBrainResponse> SubmitBrainTrainingScore([FromBody] SubmitDailyBrainRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitBrainTrainingScore");
            activity?.SetTag("score", request.m_score);
            activity?.SetTag("levels", request.m_levels);
            
            var score = Math.Min(request.m_score, m_economy.DailyBrainMaxScore);
            var scoreFac = (float)score/m_economy.DailyBrainMaxScore;
            
            var rewardMulch = (uint)(scoreFac * m_economy.DailyBrainMaxMulch);
            var rewardXp = (uint)(scoreFac * m_economy.DailyBrainMaxXp);
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + rewardMulch)
                    .SetProperty(x => x.m_xp, x => x.m_xp + rewardXp)
                );
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            
            return new SubmitDailyBrainResponse
            {
                m_modes = 2,
                m_mulchEarned = rewardMulch,
                m_xpEarned =rewardXp,
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp
            };
        }
        
        // has-the-user-played
        // save-game-stats
    }
}