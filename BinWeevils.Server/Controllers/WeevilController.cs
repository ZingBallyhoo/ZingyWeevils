using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/weevil")]
    public class WeevilController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public WeevilController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("data")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<WeevilDataResponse> GetData([FromBody] WeevilDataRequest request)
        {
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_name)
                .Select(x => new
                {
                    x.m_idx,
                    x.m_createdAt,
                    m_lastSeen = x.m_lastLogin,
                    x.m_weevilDef,
                    x.m_lastAcknowledgedLevel
                })
                .SingleOrDefaultAsync();
            if (dto == null)
            {
                return new WeevilDataResponse
                {
                    m_result = 0
                };
            }
            
            return new WeevilDataResponse
            {
                m_result = 1,
                m_idx = dto.m_idx,
                m_weevilDef = dto.m_weevilDef,
                m_level = dto.m_lastAcknowledgedLevel,
                m_tycoon = 1,
                m_lastLog = dto.m_lastSeen.ToString("yyyy-MM-dd HH:mm:ss"),
                m_dateJoined = dto.m_createdAt.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        
        [StructuredFormPost("buy-food")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyFoodResponse> BuyFood([FromBody] BuyFoodRequest request)
        {
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= request.m_cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_food, x => Math.Min(x.m_food + request.m_energyValue, 100))
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - request.m_cost));
            if (rowsUpdated == 0)
            {
                return new BuyFoodResponse
                {
                    m_success = 0
                };
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_food,
                    x.m_mulch
                })
                .SingleAsync();
            
            return new BuyFoodResponse
            {
                m_success = 1,
                m_food = dto.m_food,
                m_mulch = dto.m_mulch
            };
        }
        
        [StructuredFormPost("update-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<UpdateStatsResponse> UpdateStats([FromBody] UpdateStatsRequest request)
        {
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_food <= request.m_food)
                .Where(x => Math.Abs(x.m_fitness - request.m_fitness) <= 20)
                .Where(x => Math.Abs(x.m_happiness - request.m_happiness) <= 5)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_food, Math.Min(request.m_food, (byte)100))
                    .SetProperty(x => x.m_fitness, Math.Min(request.m_fitness, (byte)100))
                    .SetProperty(x => x.m_happiness, Math.Min(request.m_happiness, (byte)100)));
            return new UpdateStatsResponse
            {
                m_result = rowsUpdated
            };
        }
        
        [HttpGet("get-progress")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetIntroProgressResponse> GetIntroProgress()
        {
            var progress = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_introProgress)
                .SingleAsync();
            
            var encodedProgress = new EncodedIntroProgress(progress);
            return new GetIntroProgressResponse
            {
                m_result = encodedProgress.ToString()
            };
        }
        
        [StructuredFormPost("set-progress")]
        public async Task SetIntroProgress([FromBody] SetIntroProgressRequest request)
        {
            var encodedProgress = new EncodedIntroProgress(request.m_progress);
            
            // todo: validate that not progressing backwards?
            // but who cares...
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_introProgress, encodedProgress.m_bits));
        }
    }
}