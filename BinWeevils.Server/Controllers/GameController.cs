using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form;
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
        private readonly IOptionsSnapshot<EconomySettings> m_economy;
        
        public GameController(WeevilDBContext dbContext, IOptionsSnapshot<EconomySettings> economy)
        {
            m_dbContext = dbContext;
            m_economy = economy;   
        }
        
        [StructuredFormPost("submit-single")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitScoreResponse> SubmitSingle([FromBody] SubmitScoreRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GameController.SubmitSingle");
            activity?.SetTag("gameID", request.m_gameID);
            activity?.SetTag("score", request.m_score);

            // todo: this is a sandbox so there's no validation...
            
            var rewardMulch = (uint)Math.Min(request.m_score * m_economy.Value.GameScoreToMulch, m_economy.Value.MaxMulchPerGame);
            var rewardXp = (uint)Math.Min(request.m_score * m_economy.Value.GameScoreToXp, m_economy.Value.MaxXpPerGame);
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + rewardMulch)
                    .SetProperty(x => x.m_xp, x => x.m_xp + rewardXp)
                );
            
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
            
            var score = Math.Min(request.m_score, m_economy.Value.DailyBrainMaxScore);
            var scoreFac = (float)score/m_economy.Value.DailyBrainMaxScore;
            
            var rewardMulch = (uint)(scoreFac * m_economy.Value.DailyBrainMaxMulch);
            var rewardXp = (uint)(scoreFac * m_economy.Value.DailyBrainMaxXp);
            
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