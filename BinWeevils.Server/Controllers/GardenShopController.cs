using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form.Garden;
using BinWeevils.Protocol.Sql;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/gardenshop")]
    public class GardenShopController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly IOptionsSnapshot<EconomySettings> m_economySettings;
        
        public GardenShopController(WeevilDBContext dbContext, IOptionsSnapshot<EconomySettings> economySettings)
        {
            m_dbContext = dbContext;
            m_economySettings = economySettings;
        }
        
        [HttpGet("fetch")] 
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<Stock> Fetch()
        {
            using var activity = ApiServerObservability.StartActivity("GardenShopController.Fetch");
            
            IQueryable<ItemType> itemQuery = m_dbContext.m_itemTypes
                .Where(x => x.m_category == ItemCategory.Garden)
                // .Where(x => x.m_shopType == ShopType.Garden) // filter unpurchasable...
                .Where(x => x.m_price > 0)
                .OrderBy(x => x.m_ordering);
            
            var items = await itemQuery
                .ToArrayAsync();
            
            var seeds = await m_dbContext.m_seedTypes
                .Where(x => x.m_price > 0)
                .OrderBy(x => x.m_level)
                .ToArrayAsync();
            
            return new Stock
            {
                m_items = items.Select(x => new NestStockItem
                {
                    m_id = x.m_itemTypeID,
                    m_name = x.m_name,
                    m_description = x.m_description,
                    m_color = x.m_defaultHexColor,
                    m_price = m_economySettings.Value.GetItemCost(x.m_price, x.m_currency),
                    m_level = (uint)x.m_minLevel,
                    m_xp = m_economySettings.Value.GetItemXp(x.m_expPoints),
                    m_fileName = x.m_configLocation
                }).ToList(),
                m_seeds = seeds.Select(x => new SeedStockItem
                {
                    m_id = x.m_id,
                    m_level = x.m_level,
                    m_name = x.m_name,
                    m_probability = x.m_probability,
                    m_price = x.m_price,
                    m_tycoon = x.m_tycoon ? 1 : 0,
                    m_fileName = x.m_fileName,
                    m_category = (int)x.m_category,
                    m_mulchYield = x.m_mulchYield,
                    m_xpYield = x.m_xpYield,
                    m_growTime = x.m_growTime,
                    m_cycleTime = x.m_cycleTime,
                }).ToList()
            };
        }
        
        [StructuredFormPost("buy-seed")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuySeedResponse> BuySeed([FromBody] BuySeedRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenShopController.BuySeed");
            activity?.SetTag("seedTypeID", request.m_seedTypeID);
            activity?.SetTag("quantity", request.m_quantity);
            
            if (request.m_quantity <= 0)
            {
                throw new InvalidDataException("trying to buy non-positive seeds");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();

            // todo: level & tycoon check
            
            var seed = await m_dbContext.m_seedTypes
                .Where(x => x.m_id == request.m_seedTypeID)
                .Where(x => x.m_price > 0)
                .SingleOrDefaultAsync();
            if (seed == null)
            {
                return new BuySeedResponse
                {
                    m_error = 12, // ERR_CANT_BUY
                };
            }
            
            if (seed.m_category == SeedCategory.Perishable) 
            {
                if (request.m_quantity > BuySeedRequest.MAX_QUANTITY)
                {
                    throw new InvalidDataException($"trying to buy too many seeds at once");
                }
            } else
            {
                if (request.m_quantity != 1)
                {
                    throw new InvalidDataException("only allowed to buy 1 non-perishable at a time");
                }
            }
            
            var totalPrice = request.m_quantity * seed.m_price;
            var totalXp = 0; // todo: how much xp is rewarded for buy.. anything?
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= totalPrice)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - totalPrice)
                    .SetProperty(x => x.m_xp, x => x.m_xp + totalXp));
            if (rowsUpdated == 0)
            {
                return new BuySeedResponse
                {
                    m_error = 13, // ERR_CANT_AFFORD
                };
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_nest,
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            for (var i = 0; i < request.m_quantity; i++)
            {
                dto.m_nest.m_gardenSeeds.Add(new NestSeedItemDB
                {
                    m_seedTypeID = request.m_seedTypeID
                });
            }
            await m_dbContext.SaveChangesAsync();
            
            var resp = new BuySeedResponse
            {
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp,
                m_error = 1
            };
            
            await transaction.CommitAsync();
            return resp;
        }
    }
}