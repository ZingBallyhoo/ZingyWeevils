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
            
            var reward = (uint)Math.Min(request.m_score * m_economy.Value.GameScoreToMulch, m_economy.Value.MaxMulchPerGame);
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + reward));
            
            return new SubmitScoreResponse
            {
                m_mulchEarned = reward,
                m_result = SubmitScoreResponse.ERR_OK
            };
        }
        
        // has-the-user-played
        // save-game-stats
    }
}