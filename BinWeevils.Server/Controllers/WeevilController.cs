using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    public class WeevilController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public WeevilController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("weevil/buy-food")]
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
    }
}