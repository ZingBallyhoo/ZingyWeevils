using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form.Garden;
using BinWeevils.Protocol.Sql;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server.Services;
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
                    m_level = (uint)x.m_minLevel,
                    m_name = x.m_name,
                    m_probability = x.m_probability,
                    m_price = m_economySettings.Value.GetItemCost(x.m_price, x.m_currency),
                    m_tycoon = x.m_tycoonOnly ? 1 : 0,
                    m_fileName = x.m_configLocation,
                    m_xp = m_economySettings.Value.GetItemXp(x.m_expPoints),
                    m_color = x.m_defaultHexColor,
                    m_description = x.m_description,
                    m_deliveryTime = 0
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
                    m_mulchYield = m_economySettings.Value.GetPlantMulchYield(x.m_mulchYield),
                    m_xpYield = m_economySettings.Value.GetPlantXpYield(x.m_xpYield),
                    m_growTime = m_economySettings.Value.GetPlantGrowTime(x.m_growTime),
                    m_cycleTime = m_economySettings.Value.GetPlantCycleTime(x.m_cycleTime, x.m_category),
                }).ToList()
            };
        }
        
        [StructuredFormPost("buy-item")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyGardenItemResponse> BuyItem([FromBody] BuyGardenItemRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenShopController.BuyItem");
            activity?.SetTag("id", request.m_id);
            activity?.SetTag("color", request.m_color);
            
            if (request.m_color != "-1")
            {
                throw new InvalidDataException("garden items are not expected to have colors");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var item = await m_dbContext.m_itemTypes
                .Where(x => x.m_itemTypeID == request.m_id)
                .Where(x => x.m_category == ItemCategory.Garden)
                .Where(x => x.m_price > 0)
                .Select(x => new
                {
                    m_price = m_economySettings.Value.GetItemCost(x.m_price, x.m_currency),
                    m_expPoints = m_economySettings.Value.GetItemXp(x.m_expPoints),
                })
                .SingleOrDefaultAsync();
            if (item == null)
            {
                return new BuyGardenItemResponse
                {
                    m_error = 12, // ERR_CANT_BUY
                };
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= item.m_price)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - item.m_price)
                    .SetProperty(x => x.m_xp, x => x.m_xp + item.m_expPoints));
            if (rowsUpdated == 0)
            {
                return new BuyGardenItemResponse
                {
                    m_error = 13, // ERR_CANT_AFFORD
                };
            }
            
            var resultDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    m_nestID = x.m_nest.m_id,
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            await m_dbContext.m_nestGardenItems.AddAsync(new NestGardenItemDB
            {
                m_itemTypeID = request.m_id,
                m_nestID = resultDto.m_nestID
            });
            await m_dbContext.SaveChangesAsync();
            
            var resp = new BuyGardenItemResponse
            {
                m_mulch = resultDto.m_mulch,
                m_xp = resultDto.m_xp,
                m_error = 1
            };
            
            await transaction.CommitAsync();
            return resp;
        }
        
        [StructuredFormPost("buy-seed")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyGardenItemResponse> BuySeed([FromBody] BuySeedRequest request)
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
                return new BuyGardenItemResponse
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
                return new BuyGardenItemResponse
                {
                    m_error = 13, // ERR_CANT_AFFORD
                };
            }
            
            var resultDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    m_nestID = x.m_nest.m_id,
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            for (var i = 0; i < request.m_quantity; i++)
            {
                await m_dbContext.m_nestGardenSeeds.AddAsync(new NestSeedItemDB
                {
                    m_seedTypeID = request.m_seedTypeID,
                    m_nestID = resultDto.m_nestID
                });
            }
            await m_dbContext.SaveChangesAsync();
            
            var resp = new BuyGardenItemResponse
            {
                m_mulch = resultDto.m_mulch,
                m_xp = resultDto.m_xp,
                m_error = 1
            };
            
            await transaction.CommitAsync();
            return resp;
        }
    }
}