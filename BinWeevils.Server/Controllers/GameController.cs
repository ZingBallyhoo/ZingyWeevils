using System.Net.Mime;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/game")]
    public class GameController : Controller
    {
        [StructuredFormPost("submit-single")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitScoreResponse> SubmitSingle([FromBody] SubmitScoreRequest request)
        {
            var reward = Math.Min(request.m_score, 5000);
            
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