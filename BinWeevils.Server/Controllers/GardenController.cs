using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol.Form.Garden;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/garden")]
    public class GardenController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public GardenController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("get-stored-items")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<StoredGardenItems> GetStoredItems([FromBody] GetStoredGardenItemsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.GetStoredItems");
            activity?.SetTag("userID", request.m_userID);
            
            var actuallyMine = request.m_userID == ControllerContext.HttpContext.User.Identity!.Name;
            if (!actuallyMine)
            {
                // for now.. why would this be needed
                throw new InvalidDataException("request for someone else's stored garden items");
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == request.m_userID)
                .Select(weev => new
                {
                    m_items = weev.m_nest.m_gardenItems
                        .Where(item => item.m_placedItem == null)
                        .Select(item => new
                        {
                            item.m_id,
                            item.m_itemType.m_category,
                            item.m_itemType.m_powerConsumption,
                            item.m_itemType.m_configLocation
                        })
                })
                .SingleAsync();
            
            var items = new StoredGardenItems();
            foreach (var item in dto.m_items)
            {
                items.m_items.Add(new GardenInventoryItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = item.m_powerConsumption,
                    m_fileName = item.m_configLocation,
                    m_deliveryTime = 0
                });
            }
            
            return items;
        }
        
        [StructuredFormPost("move-item")]
        public async Task MoveItem([FromBody] MoveGardenItemRequest request) 
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.MoveItem");
            activity?.SetTag("itemID", request.m_itemID);
            activity?.SetTag("x", request.m_x);
            activity?.SetTag("z", request.m_z);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var validCheck = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(weev => weev.m_nest.m_gardenItems)
                .AnyAsync(item => item.m_id == request.m_itemID && item.m_placedItem != null);
            if (!validCheck)
            {
                throw new Exception("invalid move item request");
            }
            
            var dto = await m_dbContext.m_nestPlacedGardenItems
                .Where(x => x.m_id == request.m_itemID)
                .Select(x => new
                {
                    x.m_room.m_nest,
                    m_placedItem = x,
                    m_itemData = new
                    {
                        x.m_item.m_itemType.m_itemTypeID,
                        x.m_item.m_itemType.m_configLocation,
                        x.m_item.m_itemType.m_boundRadius
                    }
                })
                .SingleAsync();
            
            // todo: well... this will have to deal with plants
            var rSq = dto.m_itemData.m_boundRadius * dto.m_itemData.m_boundRadius;
            var overlapping = await m_dbContext.m_nestPlacedGardenItems
                .Where(x => x.m_id != request.m_itemID)
                .Where(x => x.m_roomID == dto.m_placedItem.m_roomID)
                .Select(other => new
                {
                    m_dSq = 
                        (other.m_x - request.m_x) * (other.m_x - request.m_x) +
                        (other.m_z - request.m_z) * (other.m_z - request.m_z),
                    m_rSq = other.m_item.m_itemType.m_boundRadius * other.m_item.m_itemType.m_boundRadius
                })
                .AnyAsync(x => x.m_dSq < rSq);
            if (overlapping)
            {
                throw new InvalidDataException("tried to place overlapping garden item");
            }
            
            dto.m_placedItem.m_x = request.m_x;
            dto.m_placedItem.m_z = request.m_z;
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("remove-item")]
        public async Task RemoveItem([FromBody] RemoveGardenItemRequest request) 
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.RemoveItem");
            activity?.SetTag("itemID", request.m_itemID);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var nest = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_nest)
                .SingleAsync();
            
            var rowsUpdated = await m_dbContext.m_nestPlacedGardenItems
                .Where(x => x.m_room.m_nestID == nest.m_id)
                .Where(x => x.m_id == request.m_itemID)
                .ExecuteDeleteAsync();
            if (rowsUpdated != 1) throw new Exception("failed to remove item from garden");
            
            nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
    }
}