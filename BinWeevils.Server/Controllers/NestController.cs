using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class NestController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public NestController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [HttpGet("nest/get-weevil-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<WeevilStatsResponse> GetWeevilStats()
        {
            var weevil = await m_dbContext.m_weevilDBs
                .SingleAsync(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name);
            
            WeevilLevels.GetLevelThresholds(weevil.m_lastAcknowledgedLevel, 
                out var xpLowerThreshold, 
                out var xpUpperThreshold);
            var stats = new WeevilStatsResponse
            {
                m_level = weevil.m_lastAcknowledgedLevel,
                m_mulch = weevil.m_mulch,
                m_xp = weevil.m_xp,
                m_xpLowerThreshold = xpLowerThreshold,
                m_xpUpperThreshold = xpUpperThreshold,
                m_food = weevil.m_food,
                m_fitness = weevil.m_fitness,
                m_happiness = weevil.m_happiness,
                m_activated = 1,
                m_daysRemaining = 99,
                m_chatState = true,
                m_chatKey = 0,
                m_serverTime = 0
            };
            
            var hashStr = string.Join("",
            [
                stats.m_level,
                stats.m_mulch,
                stats.m_xp,
                stats.m_xpLowerThreshold,
                stats.m_xpUpperThreshold,
                stats.m_food,
                stats.m_fitness,
                stats.m_happiness,
                
                stats.m_activated,
                stats.m_daysRemaining,
                
                //chatState,
                //chatKey,
                
                stats.m_serverTime
            ]);
            
            stats.m_hash = Rssmv.Hash(hashStr);
            return stats;
        }
        
        [HttpGet("nest/level-up")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<LevelUpResponse> LevelUp()
        {
            var checkDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_xp,
                    x.m_lastAcknowledgedLevel
                })
                .SingleAsync();
            
            var desiredLevel = WeevilLevels.DetermineLevel(checkDto.m_xp);
            if (desiredLevel <= checkDto.m_lastAcknowledgedLevel)
            {
                return new LevelUpResponse();
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_lastAcknowledgedLevel == checkDto.m_lastAcknowledgedLevel)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lastAcknowledgedLevel, x => x.m_lastAcknowledgedLevel + 1));
            if (rowsUpdated == 0)
            {
                return new LevelUpResponse();
            }
            
            var responseDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_lastAcknowledgedLevel,
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            
            WeevilLevels.GetLevelThresholds(responseDto.m_lastAcknowledgedLevel, 
                out var xpLowerThreshold, 
                out var xpUpperThreshold);
            var response = new LevelUpResponse
            {
                m_level = responseDto.m_lastAcknowledgedLevel,
                m_mulch = responseDto.m_mulch,
                m_xp = responseDto.m_xp,
                m_xpLowerThreshold = xpLowerThreshold,
                m_xpUpperThreshold = xpUpperThreshold,
                m_serverTime = 0
            };

            var hashStr = string.Join("",
            [
                response.m_level,
                response.m_mulch,
                response.m_xp,
                response.m_xpLowerThreshold,
                response.m_xpUpperThreshold,
                response.m_serverTime
            ]);
            
            response.m_hash = Rssmv.Hash(hashStr);
            return response;
        }
        
        [StructuredFormPost("nest/get-stored-items")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<StoredItems> GetStoredItems([FromBody] GetStoredItemsRequest request)
        {
            var actuallyMine = request.m_userID == ControllerContext.HttpContext.User.Identity!.Name;
            if (actuallyMine != request.m_mine) throw new InvalidDataException();
            
            if (!request.m_mine)
            {
                // for now.. why would this be needed
                return new StoredItems();
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name ==  ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(x => x.m_nest.m_items)
                .Where(x => x.m_placedItem == null)
                .Select(x => new
                {
                    x.m_id,
                    x.m_itemType.m_configLocation,
                    x.m_itemType.m_category
                })
                .ToArrayAsync();
            
            var storedItems = new StoredItems();
            foreach (var item in dto)
            {
                storedItems.m_items.Add(new NestInventoryItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = 0, // todo
                    m_configName = item.m_configLocation,
                    m_clrTemp = "", // todo
                    m_deliveryTime = 0, // todo
                });
            }
            
            return storedItems;
        }
        
        [StructuredFormPost("nest/getconfig")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<NestConfig> GetNestConfig([FromBody] GetNestConfigRequest request)
        {
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => new
                {
                    weev.m_idx,
                    m_nestID = weev.m_nest.m_id,
                    m_rooms = weev.m_nest.m_rooms.Select(y => new
                    {
                        y.m_type,
                        y.m_id,
                        y.m_color
                    }),
                    m_items = weev.m_nest.m_items.Where(item => item.m_placedItem != null).Select(y => new {
                        y.m_id,
                        y.m_itemType.m_configLocation,
                        y.m_itemType.m_category,
                        y.m_placedItem!.m_roomID,
                        y.m_placedItem!.m_posSnimationFrame,
                        m_placedOnFurnitureID = y.m_placedItem!.m_placedOnFurnitureID ?? 0,
                        y.m_placedItem!.m_spotOnFurniture,
                    })
                })
                .AsSplitQuery()
                .SingleAsync();
            
            var nestConfig = new NestConfig
            {
                m_id = dto.m_nestID,
                m_idx = dto.m_idx,
                m_fuel = 46807 // todo
            };
            foreach (var room in dto.m_rooms)
            {
                nestConfig.m_locs.Add(new NestConfig.Loc
                {
                    m_id = (uint)room.m_type,
                    m_instanceID = room.m_id,
                    m_colour = room.m_color
                });
            }

            foreach (var item in dto.m_items)
            {
                nestConfig.m_items.Add(new NestItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = 0, // todo
                    m_configName = item.m_configLocation,
                    m_clrTemp = "0|0|0", // todo
                    m_locID = item!.m_roomID,
                    m_currentPos = item.m_posSnimationFrame,
                    m_placedOnFurnitureID = item.m_placedOnFurnitureID,
                    m_spot = item.m_spotOnFurniture,
                });
            }
            
            return nestConfig;
        }
        
        [StructuredFormPost("php/addItemToNest.php")]
        public async Task AddItemToNest([FromBody] AddItemToNestRequest request)
        {
            // no concurrency check for placing but doesn't actually matter...
            
            if (request.m_itemID == request.m_furnitureID)
            {
                throw new InvalidDataException("could break our query");
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_nest.m_id == request.m_nestID) // dont lie :(
                .Where(x => x.m_nest.m_rooms.Any(room => room.m_id == request.m_locationID))
                .Where(x => 
                    request.m_furnitureID == 0 || 
                    x.m_nest.m_items.Any(item => 
                        item.m_id == request.m_furnitureID && 
                        item.m_placedItem != null && 
                        item.m_placedItem.m_roomID == request.m_locationID))
                .Select(x => new
                {
                    // todo: update nest modified time
                    m_item = x.m_nest.m_items.Single(item => item.m_id == request.m_itemID && item.m_placedItem == null)
                })
                .AsSplitQuery()
                .SingleAsync();
            
            uint? placedOnFurnitureID = null;
            if (request.m_furnitureID == 0)
            {
                request.m_spot = 0;
            } else
            {
                if (request.m_itemType != "o") throw new InvalidDataException();
                request.m_posAnimationFrame = 0;
                placedOnFurnitureID = request.m_furnitureID;
            }
            
            dto.m_item.m_placedItem = new NestPlacedItemDB
            {
                m_roomID = request.m_locationID,
                m_posSnimationFrame = request.m_posAnimationFrame,
                m_placedOnFurnitureID = placedOnFurnitureID,
                // if we aren't placed on furniture, "turn off" the constraint
                m_placedOnFurnitureIDShadow = placedOnFurnitureID ?? request.m_itemID,
                m_spotOnFurniture = request.m_spot
            };
            await m_dbContext.SaveChangesAsync();
        }
    }
}