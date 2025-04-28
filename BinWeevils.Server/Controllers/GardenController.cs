using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
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
                        .Select(item => new GardenInventoryItem
                        {
                            m_databaseID = item.m_id,
                            m_category = (int)item.m_itemType.m_category,
                            m_powerConsumption = item.m_itemType.m_powerConsumption,
                            m_fileName = item.m_itemType.m_configLocation,
                            m_deliveryTime = 0
                        })
                        .ToList(),
                    m_seeds = weev.m_nest.m_gardenSeeds
                        .Where(seed => seed.m_placedItem == null)
                        .Select(seed => new GardenInventorySeed
                        {
                            m_databaseID = seed.m_id,
                            m_name = seed.m_seedType.m_name,
                            m_fileName = seed.m_seedType.m_fileName,
                            m_category = (uint)seed.m_seedType.m_category,
                            m_cycleTime = seed.m_seedType.m_cycleTime,
                            m_growTime = seed.m_seedType.m_growTime,
                            m_mulch = seed.m_seedType.m_mulchYield,
                            m_xp = seed.m_seedType.m_xpYield,
                            m_radius = seed.m_seedType.m_radius,
                        })
                        .ToList()
                })
                .AsSplitQuery()
                .SingleAsync();
            
            var items = new StoredGardenItems
            {
                m_items = dto.m_items,
                m_seeds = dto.m_seeds
            };
            return items;
        }
        
        [StructuredFormPost("get-plant-configs")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<PlantConfigs> GetPlantConfigs([FromBody] GetStoredGardenItemsRequest request)
        {
            return await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == request.m_userID)
                .Select(weev => new PlantConfigs
                {
                    m_weevilHappiness = weev.m_happiness,
                    m_plants = weev.m_nest.m_gardenSeeds
                        .Where(seed => seed.m_placedItem != null)
                        .Select(seed => new GardenPlant
                        {
                            m_databaseID = seed.m_id,
                            m_name = seed.m_seedType.m_name,
                            m_fileName = seed.m_seedType.m_fileName,
                            m_category = (uint)seed.m_seedType.m_category,
                            m_cycleTime = seed.m_seedType.m_cycleTime,
                            m_growTime = seed.m_seedType.m_growTime,
                            m_mulch = seed.m_seedType.m_mulchYield,
                            m_xp = seed.m_seedType.m_xpYield,
                            m_radius = seed.m_seedType.m_radius,
                            
                            m_age = 0, // todo
                            m_watered = false, // todo
                            m_x = seed.m_placedItem!.m_x,
                            m_z = seed.m_placedItem!.m_z,
                        })
                        .ToList()
                })
                .AsSplitQuery()
                .SingleAsync();
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
                    m_placedItem = x,
                    x.m_item.m_itemType.m_boundRadius,
                    x.m_room.m_nest,
                })
                .SingleAsync();
            
            await ValidatePlacement(new ValidatePlacementData
            {
                m_nest = dto.m_nest,
                m_x = request.m_x,
                m_z = request.m_z,
                m_r = dto.m_boundRadius,
                m_thisPlantID = request.m_itemID
            });
            
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
        
        [StructuredFormPost("add-plant")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<AddPlantResponse> AddPlant([FromBody] AddPlantRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.AddPlant");
            activity?.SetTag("plantID", request.m_plantID);
            activity?.SetTag("x", request.m_x);
            activity?.SetTag("z", request.m_z);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(weev => weev.m_nest.m_gardenSeeds)
                .Where(seed => seed.m_id == request.m_plantID)
                .Where(seed => seed.m_placedItem == null)
                .Select(x => new 
                {
                    x.m_seedType.m_radius,
                    x.m_nest,
                })
                .SingleAsync();
            
            await ValidatePlacement(new ValidatePlacementData 
            {
                m_nest = dto.m_nest,
                m_x = request.m_x,
                m_z = request.m_z,
                m_r = dto.m_radius,
                m_thisPlantID = request.m_plantID
            });
            
            await m_dbContext.m_nestPlacedSeeds.AddAsync(new NestPlantDB
            {
                m_id = request.m_plantID,
                m_x = request.m_x,
                m_z = request.m_z,
                // todo: timers
            });
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // client checks that xp field is valid so don't even need to set...
            return new AddPlantResponse();
        }
        
        [StructuredFormPost("harvest-plant")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<HarvestPlantResponse> HarvestPlant([FromBody] HarvestPlantRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.HarvestPlant");
            activity?.SetTag("plantID", request.m_plantID);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => new 
                {
                    //weev.m_happiness,
                    m_plant = weev.m_nest.m_gardenSeeds
                        .Where(plant => plant.m_id == request.m_plantID)
                        .Where(plant => plant.m_placedItem != null)
                        .Select(plant => new
                        {
                            plant.m_seedType.m_mulchYield,
                            plant.m_seedType.m_xpYield,
                            //plant.m_seedType.m_growTime,
                            //plant.m_seedType.m_cycleTime,
                        })
                        .SingleOrDefault()
                })
                .SingleOrDefaultAsync();
            if (dto == null || dto.m_plant == null)
            {
                throw new Exception("invalid harvest plant request");
            }
            
            // todo: check timer
            
            var rowsDeleted = await m_dbContext.m_nestGardenSeeds
                .Where(x => x.m_id == request.m_plantID)
                .ExecuteDeleteAsync();
            if (rowsDeleted == 0)
            {
                throw new InvalidDataException("plant to harvest doesn't exist");
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + dto.m_plant.m_mulchYield)
                    .SetProperty(x => x.m_xp, x => x.m_xp + dto.m_plant.m_xpYield));
            if (rowsUpdated == 0)
            {
                throw new Exception("unable to add harvest rewards");
            }
            
            // avoid concurrency check, not needed
            // todo: the client won't reload other's plant configs so...
            /*await m_dbContext.m_nests
                .Where(x => x.m_id == nestID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lastUpdated, x => DateTime.UtcNow));*/
            
            var resultDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_mulch,
                    x.m_xp
                })
                .SingleAsync();
            
            await transaction.CommitAsync();
            
            return new HarvestPlantResponse 
            {
                m_plantID = request.m_plantID,
                m_mulch = resultDto.m_mulch,
                m_xp = resultDto.m_xp
            };
        }
        
        private struct ValidatePlacementData 
        {
            public required NestDB m_nest;
            public required short m_x;
            public required short m_z;
            public required byte m_r;
            
            public uint? m_thisPlantID;
            public uint? m_thisItemID;
        }
        
        private struct RadialBounds
        {
            private readonly short m_x;
            private readonly short m_z;
            private readonly short m_r;
            
            public RadialBounds(short x, short z, short r)
            {
                m_x = x;
                m_z = z;
                m_r = r;
            }
            
            private bool CommonCheck(short x, short z, int effectiveR) 
            {
                if (effectiveR <= 0) return false; // sanity
                
                checked 
                {
                    var dSq = 
                        (x - m_x) * (x - m_x) + 
                        (z - m_z) * (z - m_z);
                    
                    var effectiveRSq = effectiveR * effectiveR;
                    return dSq < effectiveRSq;
                }
            }
            
            public bool IsOutside(short x, short z, byte r)
            {
                return !CommonCheck(x, z, m_r - r);
            }
            
            public bool IsInside(short x, short z, byte r)
            {
                return CommonCheck(x, z, m_r + r);
            }
        }
        
        private async Task ValidatePlacement(ValidatePlacementData data) 
        {
            var noPlaceArea = new RadialBounds(5, 505, 94);
            var lawn = data.m_nest.m_gardenSize switch
            {
                EGardenSize.Regular => new RadialBounds(-3, 360, 197),
                EGardenSize.LargerGarden => new RadialBounds(36,360,236),
                EGardenSize.EvenLargerGarden => new RadialBounds(75,360,275),
                EGardenSize.DeluxeGarden => new RadialBounds(115, 360, 315),
                EGardenSize.SuperDeluxeGarden => new RadialBounds(154, 360, 354),
                _ => throw new ArgumentOutOfRangeException($"unknown garden size: {data.m_nest.m_gardenSize}")
            };
            
            if (noPlaceArea.IsInside(data.m_x, data.m_z, data.m_r))
            {
                throw new InvalidDataException("tried to place object inside of no go area");
            }
            if (lawn.IsOutside(data.m_x, data.m_z, data.m_r))
            {
                throw new InvalidDataException("tried to place object outside of lawn area");
            }
            
            var rSq = data.m_r * data.m_r;
            var overlappingWithItem = await m_dbContext.m_nestPlacedGardenItems
                .Where(x => x.m_room.m_nestID == data.m_nest.m_id)
                .Where(x => x.m_id != data.m_thisItemID)
                .Select(other => new
                {
                    m_dSq = 
                        (other.m_x - data.m_x) * (other.m_x - data.m_x) +
                        (other.m_z - data.m_z) * (other.m_z - data.m_z),
                    m_rSq = other.m_item.m_itemType.m_boundRadius * other.m_item.m_itemType.m_boundRadius
                })
                .AnyAsync(x => x.m_dSq < rSq+x.m_rSq);
            if (overlappingWithItem)
            {
                throw new InvalidDataException("tried to place overlapping a garden item");
            }
            
            var overlappingWithPlant = await m_dbContext.m_nestGardenSeeds
                .Where(x => x.m_nestID == data.m_nest.m_id)
                .Where(x => x.m_id != data.m_thisPlantID)
                .Where(x => x.m_placedItem != null)
                .Select(x => x.m_placedItem)
                .Select(other => new
                {
                    m_dSq = 
                        (other.m_x - data.m_x) * (other.m_x - data.m_x) +
                        (other.m_z - data.m_z) * (other.m_z - data.m_z),
                    m_rSq = other.m_item.m_seedType.m_radius * other.m_item.m_seedType.m_radius
                })
                .AnyAsync(x => x.m_dSq < rSq+x.m_rSq);
            if (overlappingWithPlant)
            {
                throw new InvalidDataException("tried to place overlapping a plant");
            }
        }
    }
}