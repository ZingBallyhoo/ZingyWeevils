using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Form.Nest;
using BinWeevils.Protocol.Sql;
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
        private readonly ILogger<NestController> m_logger;
        private readonly WeevilDBContext m_dbContext;
        private readonly ItemConfigRepository m_configRepo;
        private readonly NestLocationDefinitions m_locations;
        
        public NestController(ILogger<NestController> logger, WeevilDBContext dbContext, 
            ItemConfigRepository configRepo, NestLocationDefinitions locations)
        {
            m_logger = logger;
            m_dbContext = dbContext;
            m_configRepo = configRepo;
            m_locations = locations;
        }
        
        [HttpGet("nest/get-weevil-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<WeevilStatsResponse> GetWeevilStats()
        {
            using var activity = ApiServerObservability.StartActivity("NestController.GetWeevilStats");
            
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
            using var activity = ApiServerObservability.StartActivity("NestController.LevelUp");

            var checkDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_xp,
                    x.m_lastAcknowledgedLevel,
                    m_nestID = x.m_nest.m_id,
                })
                .SingleAsync();
            
            var desiredLevel = WeevilLevels.DetermineLevel(checkDto.m_xp);
            if (desiredLevel <= checkDto.m_lastAcknowledgedLevel)
            {
                m_logger.LogDebug("No level up required");
                return new LevelUpResponse();
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_lastAcknowledgedLevel == checkDto.m_lastAcknowledgedLevel)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lastAcknowledgedLevel, x => x.m_lastAcknowledgedLevel + 1));
            if (rowsUpdated == 0)
            {
                m_logger.LogError("Race while processing level up");
                return new LevelUpResponse();
            }
            
            var newLevel = checkDto.m_lastAcknowledgedLevel+1;
            m_logger.LogDebug("Levelled up from {From} to {To}", checkDto.m_lastAcknowledgedLevel, newLevel);
            
            var trophyItemName = $"o_levelTrophy{newLevel}";
            var trophyItemTypeID = await m_dbContext.FindItemByConfigName(trophyItemName);
            if (trophyItemTypeID != null)
            {
                m_logger.LogDebug("Granting level up trophy: {ItemName} ({ItemID})", trophyItemName, trophyItemTypeID);
                await m_dbContext.m_nestItems.AddAsync(new NestItemDB 
                {
                    m_nestID = checkDto.m_nestID,
                    m_itemTypeID = trophyItemTypeID.Value
                });
            }
            await m_dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
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
            using var activity = ApiServerObservability.StartActivity("NestController.GetStoredItems");
            activity?.SetTag("userID", request.m_userID);
            activity?.SetTag("mine", request.m_mine);

            var actuallyMine = request.m_userID == ControllerContext.HttpContext.User.Identity!.Name;
            if (actuallyMine != request.m_mine) throw new InvalidDataException();
            
            if (!request.m_mine)
            {
                // for now.. why would this be needed
                throw new InvalidDataException("request for someone else's stored items");
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .SelectMany(x => x.m_nest.m_items)
                .Where(x => x.m_placedItem == null)
                .Select(x => new
                {
                    x.m_id,
                    x.m_itemType.m_category,
                    x.m_itemType.m_powerConsumption,
                    x.m_itemType.m_configLocation,
                })
                .ToArrayAsync();
            
            var storedItems = new StoredItems();
            foreach (var item in dto)
            {
                storedItems.m_items.Add(new NestInventoryItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = item.m_powerConsumption,
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
            using var activity = ApiServerObservability.StartActivity("NestController.GetNestConfig");
            activity?.SetTag("userName", request.m_userName);
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userName)
                .Select(weev => new
                {
                    weev.m_idx,
                    m_isMine = weev.m_name == ControllerContext.HttpContext.User.Identity!.Name,
                    m_nestID = weev.m_nest.m_id,
                    m_lastUpdate = weev.m_nest.m_lastUpdated,
                    m_weevilXp = weev.m_xp,
                    m_gardenSize = weev.m_nest.m_gardenSize,
                    m_rooms = weev.m_nest.m_rooms.Select(y => new
                    {
                        y.m_type,
                        y.m_id,
                        y.m_color,
                        y.m_business
                    }),
                    m_items = weev.m_nest.m_items.Where(item => item.m_placedItem != null).Select(y => new
                    {
                        y.m_id,
                        y.m_itemType.m_configLocation,
                        y.m_itemType.m_category,
                        y.m_itemType.m_powerConsumption,
                        y.m_placedItem!.m_roomID,
                        y.m_placedItem!.m_posAnimationFrame,
                        m_placedOnFurnitureID = y.m_placedItem!.m_placedOnFurnitureID ?? 0,
                        y.m_placedItem!.m_spotOnFurniture,
                    }),
                    m_gardenItems = weev.m_nest.m_gardenItems.Where(x => x.m_placedItem != null).Select(y => new
                    {
                        y.m_id,
                        y.m_itemType.m_configLocation,
                        y.m_itemType.m_category,
                        y.m_itemType.m_powerConsumption,
                        y.m_itemType.m_boundRadius,
                        y.m_placedItem!.m_roomID,
                        y.m_placedItem!.m_x,
                        y.m_placedItem!.m_z,
                    }),
                    m_score = 
                        weev.m_nest.m_items.Where(item => item.m_placedItem != null).Select(x => x.m_itemType.m_coolness).Sum() +
                        weev.m_nest.m_gardenItems.Where(item => item.m_placedItem != null).Select(x => x.m_itemType.m_coolness).Sum()
                })
                .AsSplitQuery()
                .SingleAsync();
            
            var nestConfig = new NestConfig
            {
                m_id = dto.m_nestID,
                m_idx = dto.m_idx,
                m_lastUpdate = dto.m_lastUpdate.ToAs3Date(),
                m_score = (uint)dto.m_score,
                m_weevilXp = dto.m_weevilXp,
                m_gardenSize = (uint)dto.m_gardenSize,
                m_fuel = 46807 // todo
            };
            foreach (var room in dto.m_rooms)
            {
                NestConfig.Loc loc;
                if (room.m_business != null)
                {
                    loc = new NestConfig.BusinessLoc
                    {
                        m_name = room.m_business.m_name,
                        m_busOpen = room.m_business.m_open,
                        m_busType = (uint)room.m_business.m_type,
                        m_signColour = room.m_business.m_signColor,
                        m_signTextColour = room.m_business.m_signTextColor, 
                        m_playList = "" // todo
                    };
                } else
                {
                    loc = new NestConfig.Loc();
                }
                
                loc.m_id = dto.m_isMine ? (int)room.m_type : -(int)room.m_type;
                loc.m_instanceID = room.m_id;
                loc.m_color = room.m_color;
                
                nestConfig.m_locs.Add(loc);
            }

            foreach (var item in dto.m_items)
            {
                nestConfig.m_items.Add(new NestItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = item.m_powerConsumption,
                    m_configName = item.m_configLocation,
                    m_clrTemp = "0|0|0", // todo
                    m_locID = item.m_roomID,
                    m_currentPos = item.m_posAnimationFrame,
                    m_placedOnFurnitureID = item.m_placedOnFurnitureID,
                    m_spot = item.m_spotOnFurniture,
                });
            }
            foreach (var item in dto.m_gardenItems)
            {
                nestConfig.m_items.Add(new GardenItem
                {
                    m_databaseID = item.m_id,
                    m_category = (int)item.m_category,
                    m_powerConsumption = item.m_powerConsumption,
                    m_fileName = item.m_configLocation,
                    m_locID = item.m_roomID,
                    m_x = item.m_x,
                    m_z = item.m_z,
                    m_r = item.m_boundRadius
                });
            }
            
            return nestConfig;
        }
        
        [StructuredFormPost("nest/get-nest-state")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetNestStateResponse> GetNestState([FromBody] GetNestStateRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.GetNestState");
            activity?.SetTag("userName", request.m_userName);
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userName)
                .Select(weev => new
                {
                    weev.m_xp,
                    weev.m_nest.m_lastUpdated,
                    m_score = weev.m_nest.m_items.Where(item => item.m_placedItem != null).Select(x => x.m_itemType.m_coolness).Sum()
                })
                .SingleOrDefaultAsync();
            
            if (dto == null)
            {
                return new GetNestStateResponse();
            }
            
            return new GetNestStateResponse
            {
                m_responseCode = 1,
                m_error = "OK",
                m_weevilXp = dto.m_xp,
                m_score = dto.m_score,
                m_fuel = 222, // todo
                m_lastUpdate = dto.m_lastUpdated.ToAs3Date(),
            };
        }
        
        [StructuredFormPost("php/addItemToNest.php")]
        public async Task AddItemToNest([FromBody] AddItemToNestRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.AddItemToNest");
            activity?.SetTag("userName", request.m_userName);
            activity?.SetTag("nestID", request.m_nestID);
            activity?.SetTag("locID", request.m_locationID);
            activity?.SetTag("itemID", request.m_itemID);
            activity?.SetTag("itemType", request.m_itemType);
            activity?.SetTag("posAnimationFrame", request.m_posAnimationFrame);
            activity?.SetTag("furnitureID", request.m_furnitureID);
            activity?.SetTag("spot", request.m_spot);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            if (request.m_userName != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to remove an item from someone else's nest");
            }
            if (request.m_itemID == request.m_furnitureID)
            {
                throw new InvalidDataException("could break our query");
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_nest.m_id == request.m_nestID) // dont lie :(
                //.Where(x => x.m_nest.m_rooms.Any(room => room.m_id == request.m_locationID))
                .Where(x => 
                    request.m_furnitureID == 0 || 
                    x.m_nest.m_items.Any(item => 
                        item.m_id == request.m_furnitureID && 
                        item.m_placedItem != null && 
                        item.m_placedItem.m_roomID == request.m_locationID))
                .Select(x => new
                {
                    x.m_nest,
                    m_placedOnItemData = x.m_nest.m_items
                        .Where(item => item.m_id == request.m_furnitureID &&
                                       item.m_placedItem != null &&
                                       item.m_placedItem.m_roomID == request.m_locationID)
                        .Select(item => new
                        {
                            item.m_itemType.m_configLocation
                        })
                        .SingleOrDefault(),
                    m_itemData = x.m_nest.m_items
                        .Where(item => item.m_id == request.m_itemID && item.m_placedItem == null)
                        .Select(item => new
                        {
                            m_item = item,
                            item.m_itemType.m_itemTypeID,
                            item.m_itemType.m_category,
                            item.m_itemType.m_configLocation,
                        })
                        .Single(),
                    m_roomType = x.m_nest.m_rooms
                        .Where(room => room.m_id == request.m_locationID)
                        .Select(room => room.m_type).Single()
                })
                .AsSplitQuery()
                .SingleAsync();
            
            var validateParams = new NormalizeItemParams
            {
                m_itemType = request.m_itemType,
                m_posAnimationFrame = request.m_posAnimationFrame,
                m_placedOnFurnitureID = request.m_furnitureID,
                m_spot = request.m_spot,
                
                m_configLocation = dto.m_itemData.m_configLocation,
                m_placedOnConfigLocation = dto.m_placedOnItemData?.m_configLocation
            };
            await NormalizeItemPlacement(validateParams); 
            
            var locDef = m_locations.m_locations.Single(x => x.m_id == (int)dto.m_roomType);
            var locCategory = (ItemCategory)locDef.m_category;
            if (dto.m_itemData.m_category != locCategory)
            {
                throw new InvalidDataException($"trying to place {dto.m_itemData.m_configLocation} in wrong room type. {locCategory} vs {dto.m_itemData.m_category}");
            }
            
            dto.m_itemData.m_item.m_placedItem = new NestPlacedItemDB
            {
                m_roomID = request.m_locationID,
                m_posAnimationFrame = validateParams.m_posAnimationFrame,
                m_placedOnFurnitureID = validateParams.m_placedOnFurnitureID != 0 ? 
                    validateParams.m_placedOnFurnitureID : null,
                // if we aren't placed on furniture, "turn off" the constraint
                m_posIdentity = validateParams.m_placedOnFurnitureID != 0 ? 
                    validateParams.m_placedOnFurnitureID : dto.m_itemData.m_itemTypeID,
                m_spotOnFurniture = validateParams.m_spot
            };
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        
        private class NormalizeItemParams
        {
            public string m_itemType;
            public byte m_posAnimationFrame;
            public uint m_placedOnFurnitureID;
            public byte m_spot;
            
            public string m_configLocation;
            public string? m_placedOnConfigLocation;
        }
        
        private async Task NormalizeItemPlacement(NormalizeItemParams para)
        {
            var itemConfig = await m_configRepo.GetConfig(para.m_configLocation);
            
            var isFurniture = itemConfig.m_type == "furniture";
            var isOrnament = itemConfig.m_type == "ornament";
            
            var expectedType = itemConfig.m_type switch
            {
                "furniture" => "f",
                "ornament" => "o",
                _ => "d"
            };
            if (expectedType != para.m_itemType)
            {
                throw new InvalidDataException($"lying about item type of {para.m_configLocation}");
            }
            
            if (isFurniture)
            {
                if (itemConfig.m_positions.All(x => x.m_frame != para.m_posAnimationFrame))
                {
                    throw new InvalidDataException($"requesting a frame of {para.m_configLocation} that doesn't exist ({para.m_posAnimationFrame})");
                }
            } else
            {
                para.m_posAnimationFrame = 0;
            }
            
            if (isOrnament != (para.m_placedOnFurnitureID != 0))
            {
                throw new InvalidDataException($"lying about ornament status of {para.m_configLocation}");
            }
            if (para.m_placedOnFurnitureID == 0)
            {
                para.m_spot = 0;
            } else
            {
                if (para.m_placedOnConfigLocation == null)
                {
                    throw new InvalidDataException($"trying to place {para.m_configLocation} on invalid furniture");
                }
                
                var placedOnItemConfig = await m_configRepo.GetConfig(para.m_placedOnConfigLocation);
                if (para.m_spot >= placedOnItemConfig.m_numSpots)
                {
                    throw new InvalidDataException($"trying to place {para.m_configLocation} on invalid spot of {para.m_placedOnConfigLocation}. {para.m_spot} vs {placedOnItemConfig.m_numSpots}");
                }
                if (placedOnItemConfig.m_type != "furniture")
                {
                    throw new InvalidDataException($"trying to place {para.m_configLocation} on non-furniture item {para.m_placedOnConfigLocation}");
                }
                
                para.m_posAnimationFrame = 0;
            }
        }
        
        [StructuredFormPost("php/updateItemPosition.php")]
        public async Task UpdateItemPosition([FromBody] UpdateNestItemPositionRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.UpdateItemPosition");
            activity?.SetTag("userName", request.m_userName);
            activity?.SetTag("nestID", request.m_nestID);
            activity?.SetTag("itemID", request.m_itemID);
            activity?.SetTag("itemType", request.m_itemType);
            activity?.SetTag("posAnimationFrame", request.m_posAnimationFrame);
            activity?.SetTag("furnitureID", request.m_furnitureID);
            activity?.SetTag("spot", request.m_spot);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var validCheck = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(weev => weev.m_nest.m_id == request.m_nestID)
                .SelectMany(weev => weev.m_nest.m_items)
                .AnyAsync(item => item.m_id == request.m_itemID && item.m_placedItem != null);
            if (!validCheck)
            {
                throw new Exception("invalid update item request");
            }
            
            var dto = await m_dbContext.m_nestPlacedItems
                .Where(x => x.m_id == request.m_itemID)
                .Select(x => new
                {
                    x.m_room.m_nest,
                    m_placedOnItemData = x.m_room.m_nest.m_items
                        .Where(item => item.m_id == request.m_furnitureID &&
                                       item.m_placedItem != null &&
                                       item.m_placedItem.m_roomID == x.m_roomID)
                        .Select(item => new
                        {
                            item.m_itemType.m_configLocation
                        })
                        .SingleOrDefault(),
                    m_placedItem = x,
                    m_itemData = new
                    {
                        x.m_item.m_itemType.m_itemTypeID,
                        x.m_item.m_itemType.m_category,
                        x.m_item.m_itemType.m_configLocation,
                    }
                })
                .SingleAsync();
                
            var validateParams = new NormalizeItemParams
            {
                m_itemType = request.m_itemType,
                m_posAnimationFrame = request.m_posAnimationFrame,
                m_placedOnFurnitureID = request.m_furnitureID,
                m_spot = request.m_spot,
                
                m_configLocation = dto.m_itemData.m_configLocation,
                m_placedOnConfigLocation = dto.m_placedOnItemData?.m_configLocation
            };
            await NormalizeItemPlacement(validateParams); 
            
            await m_dbContext.m_nestPlacedItems
                .Where(x => x.m_id == request.m_itemID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_posAnimationFrame, validateParams.m_posAnimationFrame)
                    .SetProperty(x => x.m_placedOnFurnitureID, validateParams.m_placedOnFurnitureID != 0 ? 
                         validateParams.m_placedOnFurnitureID : null)
                    .SetProperty(x => x.m_posIdentity, validateParams.m_placedOnFurnitureID != 0 ? 
                         validateParams.m_placedOnFurnitureID : dto.m_itemData.m_itemTypeID)
                    .SetProperty(x => x.m_spotOnFurniture, validateParams.m_spot)
                );
            
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("php/removeItemFromNest.php")]
        public async Task RemoveItemFromNest([FromBody] RemoveItemFromNestRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.UpdateItemPosition");
            activity?.SetTag("userName", request.m_userName);
            activity?.SetTag("nestID", request.m_nestID);
            activity?.SetTag("itemID", request.m_itemID);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();

            if (request.m_userName != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to remove an item from someone else's nest");
            }
            
            // todo: use a claim for nest too?
            var nest = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_nest)
                .SingleAsync();
            if (nest.m_id != request.m_nestID)
            {
                throw new Exception("sent wrong nest id");
            }
            
            var rowsUpdated = await m_dbContext.m_nestPlacedItems
                .Where(x => x.m_id == request.m_itemID)
                .Where(x => x.m_room.m_nestID == request.m_nestID)
                .ExecuteDeleteAsync();
            if (rowsUpdated != 1) throw new Exception("failed to remove item from nest");
            
            nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("php/setLocColour.php")]
        public async Task SetLocColor([FromBody] SetLocColorRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.SetLocColor");
            activity?.SetTag("nestID", request.m_nestID);
            activity?.SetTag("locID", request.m_locID);
            activity?.SetTag("col", request.m_col);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();

            var nest = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => weev.m_nest)
                .Where(nest => nest.m_id == request.m_nestID)
                .Where(nest => nest.m_rooms.Any(room => room.m_id == request.m_locID))
                .SingleAsync();
            
            var parsedCol = NestRoomColor.Parse(request.m_col, null);
            await m_dbContext.m_nestRooms
                .Where(x => x.m_id == request.m_locID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_color, parsedCol));
            
            nest.m_lastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("nest/purchase-nest-room")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<PurchaseNestRoomResponse> PurchaseNestRoom([FromBody] PurchaseNestRoomRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("NestController.PurchaseNestRoom");
            activity?.SetTag("roomType", request.m_roomType);
            
            var price = request.m_roomType switch
            {
                ENestRoom.Room1 => 10000, // NW_Room
                ENestRoom.Room2 => 10000, // N_Room
                ENestRoom.Room3 => 10000, // NE_Room
                // 4 = W_Room
                
                ENestRoom.Room6 => 8000, // E_Room
                ENestRoom.Room7 => 8000, // SW_Room
                ENestRoom.Room8 => 10000, // S_Room
                ENestRoom.Room9 => 8000, // SE_Room
                
                _ => throw new InvalidDataException($"room type {request.m_roomType} is not purchasable")
            };
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= price)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - price)
                    .SetProperty(x => x.m_xp, x => x.m_xp + 1000));
            if (rowsUpdated == 0)
            {
                return new PurchaseNestRoomResponse 
                {
                    m_error = PurchaseNestRoomResponse.ERROR_CANT_AFFORD
                };
            }
            
            var nest = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => weev.m_nest)
                .SingleAsync();
            
            var newRoom = new NestRoomDB
            {
                m_type = request.m_roomType
            };
            nest.m_rooms.Add(newRoom);
            nest.m_lastUpdated = DateTime.UtcNow;
            
            try
            {
                await m_dbContext.SaveChangesAsync();
            } catch (DbUpdateException)
            {
                return new PurchaseNestRoomResponse 
                {
                    m_error = PurchaseNestRoomResponse.ERROR_ALREADY_OWNED
                };
            }
            
            await transaction.CommitAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            return new PurchaseNestRoomResponse
            {
                m_error = PurchaseNestRoomResponse.ERROR_OK,
                m_locID = newRoom.m_id,
                m_mulch = dto.m_mulch,
                m_xp = dto.m_xp
            };
        }
    }
}