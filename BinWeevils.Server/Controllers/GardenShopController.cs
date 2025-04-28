using System.Net.Mime;
using BinWeevils.Database;
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
    }
}