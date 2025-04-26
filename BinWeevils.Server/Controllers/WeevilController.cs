using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
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
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetData");
            activity?.SetTag("name", request.m_name);
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_name)
                .Select(x => new
                {
                    x.m_idx,
                    x.m_createdAt,
                    x.m_lastLogin,
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
                m_lastLog = dto.m_lastLogin.ToAs3Date(),
                m_dateJoined = dto.m_createdAt.ToAs3Date()
            };
        }
        
        [StructuredFormPost("buy-food")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyFoodResponse> BuyFood([FromBody] BuyFoodRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.BuyFood");
            activity?.SetTag("cost", request.m_cost);
            activity?.SetTag("energyValue", request.m_energyValue);
            
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
            using var activity = ApiServerObservability.StartActivity("WeevilController.UpdateStats");

            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_food >= request.m_food) // can only decrease
                .Where(x => Math.Abs(x.m_fitness - request.m_fitness) <= 20)
                .Where(x => Math.Abs(x.m_happiness - request.m_happiness) <= 10)
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
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetIntroProgress");

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
            using var activity = ApiServerObservability.StartActivity("WeevilController.SetIntroProgress");

            var encodedProgress = new EncodedIntroProgress(request.m_progress);
            
            // todo: validate that not progressing backwards?
            // but who cares...
            
            await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_introProgress, encodedProgress.m_bits));
        }
        
        [StructuredFormPost("change-definition")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<ChangeWeevilDefResponse> ChangeDefinition([FromBody] ChangeWeevilDefRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.ChangeDefinition");

            var def = new WeevilDef($"{request.m_weevilDef}");
            if (!def.ValidateLegacy())
            {
                throw new InvalidDataException($"ChangeDefinition: invalid weevil def: \"{request.m_weevilDef}\"");
            }
            
            const int cost = 2500;
            var defNum = def.AsNumber();
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - cost)
                    .SetProperty(x => x.m_weevilDef, defNum));
            
            if (rowsUpdated == 0)
            {
                return new ChangeWeevilDefResponse
                {
                    m_error = ChangeWeevilDefResponse.ERR_NOT_ENOUGH_MONEY
                };
            }
            return new ChangeWeevilDefResponse
            {
                m_error = ChangeWeevilDefResponse.ERR_OK
            };
        }
    }
}