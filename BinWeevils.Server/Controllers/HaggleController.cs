using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Form;
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
    [Route("api")]
    public class HaggleController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly EconomySettings m_economySettings;
        
        public HaggleController(WeevilDBContext dbContext, IOptionsSnapshot<EconomySettings> enconomySettings)
        {
            m_dbContext = dbContext;
            m_economySettings = enconomySettings.Value;
        }
        
        [HttpGet("nest/get-all-stored")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<StoredHaggleItems> GetAllStoredItems()
        {
            using var activity = ApiServerObservability.StartActivity("HaggleController.GetAllStoredItems");
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_nest)
                .Select(nest => new
                {
                    m_items = nest.m_items.Where(item => item.m_placedItem == null)
                        .Where(x => x.m_itemType.m_price > 0)
                        .Where(x => x.m_itemType.m_canDelete)
                        .Select(x => new HaggleItem
                    {
                        m_databaseID = x.m_id, 
                        m_type = (byte)EHaggleItemType.NestItem,
                        m_color = x.m_color,
                        m_configLocation = x.m_itemType.m_configLocation,
                        m_value = m_economySettings.GetItemCost(x.m_itemType.m_price, x.m_itemType.m_currency),
                    }).ToArray(),
                    m_gardenItems = nest.m_gardenItems.Where(x => x.m_placedItem == null)
                        .Where(x => x.m_itemType.m_price > 0)
                        .Where(x => x.m_itemType.m_canDelete)
                        .Select(x =>new HaggleItem
                    {
                        m_databaseID = x.m_id, 
                        m_type = (byte)EHaggleItemType.GardenItem,
                        m_color = x.m_color,
                        m_configLocation = x.m_itemType.m_configLocation,
                        m_value = m_economySettings.GetItemCost(x.m_itemType.m_price, x.m_itemType.m_currency),
                    }).ToArray()
                })
                .AsSplitQuery()
                .SingleAsync();
            
            var resultItems = new List<HaggleItem>();
            foreach (var item in dto.m_items.Concat(dto.m_gardenItems))
            {
                // todo: needed?
                // most things just have 0 price
                
                //var itemConfig = await m_configRepo.GetConfig(item.m_configLocation);
                //if (itemConfig.m_noSell) continue;
                
                resultItems.Add(item);
            }
            
            return new StoredHaggleItems
            {
                m_items = resultItems
            };
        }
        
        [StructuredFormPost("shop/haggle")] 
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<HaggleSellResponse> SellHaggle([FromBody] HaggleSellRequest request) 
        {
            using var activity = ApiServerObservability.StartActivity("HaggleController.SellHaggle");
            activity?.SetTag("type", request.m_type);
            activity?.SetTag("nestItems", request.m_nestItems);
            activity?.SetTag("gardenItems", request.m_gardenItems);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();

            var initDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new 
                {
                    x.m_idx,
                    m_nestID = x.m_nest.m_id
                })
                .SingleAsync();
            
            var totalValue = 0u;
            foreach (var itemID in request.m_nestItems)
            {
                var itemDto = await m_dbContext.m_nestItems
                    .Where(x => x.m_nestID == initDto.m_nestID)
                    .Where(x => x.m_id == itemID)
                    .Where(x => x.m_placedItem == null)
                    .Where(x => x.m_itemType.m_price > 0)
                    .Where(x => x.m_itemType.m_canDelete)
                    .Select(x => new
                    {
                        m_value = m_economySettings.GetItemCost(x.m_itemType.m_price, x.m_itemType.m_currency)
                    })
                    .SingleAsync();
                
                await m_dbContext.m_nestItems
                    .Where(x => x.m_id == itemID)
                    .Where(x => x.m_placedItem == null)
                    .ExecuteDeleteAsync();
                
                totalValue += itemDto.m_value;
            }
            
            foreach (var itemID in request.m_gardenItems)
            {
                var itemDto = await m_dbContext.m_nestGardenItems
                    .Where(x => x.m_nestID == initDto.m_nestID)
                    .Where(x => x.m_id == itemID)
                    .Where(x => x.m_placedItem == null)
                    .Where(x => x.m_itemType.m_price > 0)
                    .Where(x => x.m_itemType.m_canDelete)
                    .Select(x => new
                    {
                        m_value = m_economySettings.GetItemCost(x.m_itemType.m_price, x.m_itemType.m_currency)
                    })
                    .SingleAsync();
                
                await m_dbContext.m_nestGardenItems
                    .Where(x => x.m_id == itemID)
                    .Where(x => x.m_placedItem == null)
                    .ExecuteDeleteAsync();
                
                totalValue += itemDto.m_value;
            }
            
            var hagglePriceDecimal = request.m_type switch
            {
                EHaggleSaleType.Default => totalValue * 0.2,
                EHaggleSaleType.GambleLow => totalValue * 0.1,
                EHaggleSaleType.GambleOkay => totalValue * 0.15,
                EHaggleSaleType.GambleBest => totalValue * 0.35,
                _ => throw new InvalidDataException("unknown haggle sale type")
            };
            var hagglePrice = (uint)Math.Floor(hagglePriceDecimal);
            activity?.SetTag("hagglePrice", hagglePrice);
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == initDto.m_idx)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + hagglePrice));
            if (rowsUpdated == 0) 
            {
                throw new Exception("unable to give haggle mulch");
            }
            
            var resultDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == initDto.m_idx)
                .Select(x => new
                {
                    x.m_mulch,
                })
                .SingleAsync();
            
            await transaction.CommitAsync();
            
            ApiServerObservability.s_haggleItemsSold.Add(request.m_nestItems.Count);
            ApiServerObservability.s_haggleTotalPayout.Add(hagglePrice);
            
            return new HaggleSellResponse
            {
                m_error = 1,
                m_newMulch = resultDto.m_mulch
            };
        }
    }
}