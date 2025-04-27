using BinWeevils.Database;
using BinWeevils.Protocol.Form.Garden;
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
    }
}