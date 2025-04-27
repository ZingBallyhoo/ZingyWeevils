using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form;
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
    [Route("api")]
    public class ShopController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly IOptionsSnapshot<EconomySettings> m_economySettings;
        
        public ShopController(WeevilDBContext dbContext, IOptionsSnapshot<EconomySettings> economySettings)
        {
            m_dbContext = dbContext;
            m_economySettings = economySettings;
        }
        
        [HttpGet("shop/fetch/type/{shopType}")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<Stock> GetStock(string shopType)
        {
            using var activity = ApiServerObservability.StartActivity("ShopController.GetStock");
            activity?.SetTag("shopType", shopType);

            IQueryable<ItemType> itemQuery = m_dbContext.m_itemTypes
                .Where(x => x.m_price > 0)
                .OrderBy(x => x.m_itemTypeID);
            
            itemQuery = shopType switch
            {
                "furniture" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.nestco)
                    .Where(x => x.m_configLocation.StartsWith("f_")),
                "gadget" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.nestco)
                    .Where(x => x.m_configLocation.StartsWith("o_")),
                "floorsAndWalls" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.nestco)
                    .Where(x => x.m_configLocation.StartsWith("rug") || 
                                x.m_configLocation.StartsWith("wallpaper") || 
                                x.m_configLocation.StartsWith("floor") ||
                                x.m_configLocation.StartsWith("ceiling")),
                "tycoon" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.tycoon),
                "nightClub" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.nightClub),
                "photoStudio" => itemQuery
                    .Where(x => x.m_shopType == ItemShopType.photoStudio)
            };
            
            var items = await itemQuery
                .Take(100)
                .ToArrayAsync();
            
            return new Stock
            {
                m_items = items.Select(x => new StockItem
                {
                    m_id = x.m_itemTypeID,
                    m_name = x.m_name,
                    m_description = x.m_description,
                    m_color = x.m_defaultHexColor,
                    m_price = m_economySettings.Value.GetItemCost(x.m_price, x.m_currency),
                    m_level = (uint)x.m_minLevel,
                    m_xp = m_economySettings.Value.GetItemXp(x.m_expPoints),
                    m_fileName = x.m_configLocation
                }).ToList()
            };
        }
        
        [StructuredFormPost("shop/buyitem")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyItemResponse> BuyItem([FromBody] BuyItemRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("ShopController.BuyItem");
            activity?.SetTag("id", request.m_id);
            activity?.SetTag("itemColor", request.m_itemColor);
            activity?.SetTag("shop", request.m_shop);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            // todo: check level?
            
            var itemToBuy = await m_dbContext.m_itemTypes
                .Where(x => x.m_price > 0)
                .Where(x => x.m_category != ItemCategory.Garden) // please don't
                .SingleAsync(x => x.m_itemTypeID == request.m_id);
            var itemCost = m_economySettings.Value.GetItemCost(itemToBuy.m_price, itemToBuy.m_currency);
            var itemXp = m_economySettings.Value.GetItemXp(itemToBuy.m_expPoints);
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= itemCost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - itemCost)
                    .SetProperty(x => x.m_xp, x => x.m_xp + itemXp));
            if (rowsUpdated == 0)
            {
                return new BuyItemResponse
                {
                    m_result = 2, // ERR_CANT_AFFORD
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
            dto.m_nest.m_items.Add(new NestItemDB
            {
                m_itemType = itemToBuy
            });
            await m_dbContext.SaveChangesAsync();
            
            var resp = new BuyItemResponse
            {
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp,
                m_result = 1
            };
            
            await transaction.CommitAsync();
            return resp;
        }
    }
    
    // itemType:
    // "ceiling"
    // "carpet"
    // "rug"
    // "wallpaper"
    // "ornament"
}