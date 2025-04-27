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
        
        // has-the-user-played
        // save-game-stats
    }
}