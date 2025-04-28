using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
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
    [Route("api/garden")]
    public class GardenController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly IOptionsSnapshot<EconomySettings> m_economySettings;
        private readonly TimeProvider m_timeProvider;
        
        public GardenController(WeevilDBContext dbContext, IOptionsSnapshot<EconomySettings> economySettings, 
            TimeProvider timeProvider)
        {
            m_dbContext = dbContext;
            m_economySettings = economySettings;
            m_timeProvider = timeProvider;
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
                            m_cycleTime = m_economySettings.Value.GetPlantCycleTime(seed.m_seedType.m_cycleTime, seed.m_seedType.m_category),
                            m_growTime = m_economySettings.Value.GetPlantGrowTime(seed.m_seedType.m_growTime),
                            m_mulch = m_economySettings.Value.GetPlantMulchYield(seed.m_seedType.m_mulchYield),
                            m_xp = m_economySettings.Value.GetPlantXpYield(seed.m_seedType.m_xpYield),
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
                            m_cycleTime = m_economySettings.Value.GetPlantCycleTime(seed.m_seedType.m_cycleTime, seed.m_seedType.m_category),
                            m_growTime = m_economySettings.Value.GetPlantGrowTime(seed.m_seedType.m_growTime),
                            m_mulch = m_economySettings.Value.GetPlantMulchYield(seed.m_seedType.m_mulchYield),
                            m_xp = m_economySettings.Value.GetPlantXpYield(seed.m_seedType.m_xpYield),
                            m_radius = seed.m_seedType.m_radius,
                            
                            m_age = GetPlantAge(m_timeProvider, seed.m_placedItem!.m_growthStartTime),
                            m_watered = false, // why would the server set this...
                            m_x = seed.m_placedItem!.m_x,
                            m_z = seed.m_placedItem!.m_z,
                        })
                        .ToList()
                })
                .AsSplitQuery()
                .SingleAsync();
        }
        
        [StructuredFormPost("add-item")]
        public async Task AddItem([FromBody] AddGardenItemRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.AddItem");
            activity?.SetTag("locID", request.m_locID);
            activity?.SetTag("itemID", request.m_itemID);
            activity?.SetTag("x", request.m_x);
            activity?.SetTag("z", request.m_z);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => new 
                {
                    weev.m_nest,
                    m_boundsRadius = weev.m_nest.m_items
                        .Where(x => x.m_id == request.m_itemID)
                        .Where(x => x.m_placedItem == null)
                        .Select(x => x.m_itemType.m_boundRadius)
                        .Single(),
                    m_gardenLocID = weev.m_nest.m_rooms
                        .Where(x => x.m_type == ENestRoom.Garden)
                        .Select(x => x.m_id)
                        .Single()
                })
                .SingleAsync();
            if (dto.m_gardenLocID != request.m_locID) 
            {
                throw new Exception("trying to add garden item in wrong loc");
            }
            
            await ValidatePlacement(new ValidatePlacementData
            {
                m_nest = dto.m_nest,
                m_x = request.m_x,
                m_z = request.m_z,
                m_r = dto.m_boundsRadius,
                m_thisItemID = request.m_itemID
            });
            
            await m_dbContext.m_nestPlacedGardenItems.AddAsync(new NestPlacedGardenItemDB
            {
                m_id = request.m_itemID,
                m_roomID = request.m_locID,
                m_x = request.m_x,
                m_z = request.m_z,
            });
            
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
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
                m_thisItemID = request.m_itemID
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
                m_growthStartTime = m_timeProvider.GetUtcNow()
            });
            
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow; // concurrency check
            await m_dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // client checks that xp field is valid so don't even need to set...
            return new AddPlantResponse();
        }
        
        [StructuredFormPost("move-plant")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task MovePlant([FromBody] AddPlantRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.MovePlant");
            activity?.SetTag("plantID", request.m_plantID);
            activity?.SetTag("x", request.m_x);
            activity?.SetTag("z", request.m_z);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(weev => weev.m_nest.m_gardenSeeds)
                .Where(seed => seed.m_id == request.m_plantID)
                .Where(seed => seed.m_placedItem != null)
                .Select(x => new 
                {
                    x.m_seedType.m_category,
                    x.m_seedType.m_radius,
                    x.m_nest,
                })
                .SingleAsync();
            if (dto.m_category == SeedCategory.Perishable)
            {
                throw new Exception("trying to move a perishable plant");
            }
            
            await ValidatePlacement(new ValidatePlacementData 
            {
                m_nest = dto.m_nest,
                m_x = request.m_x,
                m_z = request.m_z,
                m_r = dto.m_radius,
                m_thisPlantID = request.m_plantID
            });
            
            await m_dbContext.m_nestPlacedSeeds
                .Where(plant => plant.m_id == request.m_plantID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_x, request.m_x)
                    .SetProperty(x => x.m_z, request.m_z)
                );
            
            dto.m_nest.m_itemsLastUpdated = DateTime.UtcNow; // concurrency check
            await m_dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("water-plant")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task WaterPlant([FromBody] HarvestPlantRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("GardenController.WaterPlant");
            activity?.SetTag("plantID", request.m_plantID);
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(weev => new 
                {
                    weev.m_happiness,
                    m_plant = weev.m_nest.m_gardenSeeds
                        .Where(plant => plant.m_id == request.m_plantID)
                        .Where(plant => plant.m_placedItem != null)
                        .Where(seed => seed.m_seedType.m_category == SeedCategory.Perishable)
                        .Select(plant => new
                        {
                            plant.m_seedType.m_category,
                            m_cycleTime = m_economySettings.Value.GetPlantCycleTime(plant.m_seedType.m_cycleTime, plant.m_seedType.m_category),
                            m_growTime = m_economySettings.Value.GetPlantGrowTime(plant.m_seedType.m_growTime),
                            plant.m_placedItem!.m_growthStartTime,
                        })
                        .SingleOrDefault()
                }).SingleOrDefaultAsync();
            
            if (dto == null || dto.m_plant == null)
            {
                throw new Exception("invalid water plant request");
            }
            
            var state = GetPlantState(new PlantStateData
            {
                m_weevilHappiness = dto.m_happiness,
                m_category = dto.m_plant.m_category,
                m_growTime = dto.m_plant.m_growTime,
                m_cycleTime = dto.m_plant.m_cycleTime,
                m_growthStartTime = dto.m_plant.m_growthStartTime,
            });
            if (state != PlantState.Harvestable)
            {
                throw new Exception($"attempt to water a plant in the wrong state: {state}");
            }
            
            // todo: why does client use +2 here? (and -2 elsewhere)
            var newGrowthStart = m_timeProvider.GetUtcNow() - TimeSpan.FromMinutes(dto.m_plant.m_growTime);
            var rowsUpdated = await m_dbContext.m_nestPlacedSeeds
                .Where(x => x.m_id == request.m_plantID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_growthStartTime, newGrowthStart));
            if (rowsUpdated == 0)
            {
                throw new InvalidDataException("plant to water doesn't exist (race)");
            }
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
                    weev.m_happiness,
                    m_plant = weev.m_nest.m_gardenSeeds
                        .Where(plant => plant.m_id == request.m_plantID)
                        .Where(plant => plant.m_placedItem != null)
                        .Select(plant => new
                        {
                            plant.m_seedType.m_category,
                            m_mulchYield = m_economySettings.Value.GetPlantMulchYield(plant.m_seedType.m_mulchYield),
                            m_xpYield = m_economySettings.Value.GetPlantXpYield(plant.m_seedType.m_xpYield),
                            m_cycleTime = m_economySettings.Value.GetPlantCycleTime(plant.m_seedType.m_cycleTime, plant.m_seedType.m_category),
                            m_growTime = m_economySettings.Value.GetPlantGrowTime(plant.m_seedType.m_growTime),
                            plant.m_placedItem!.m_growthStartTime,
                        })
                        .SingleOrDefault()
                })
                .SingleOrDefaultAsync();
            if (dto == null || dto.m_plant == null)
            {
                throw new Exception("invalid harvest plant request");
            }
            
            var state = GetPlantState(new PlantStateData
            {
                m_weevilHappiness = dto.m_happiness,
                m_category = dto.m_plant.m_category,
                m_growTime = dto.m_plant.m_growTime,
                m_cycleTime = dto.m_plant.m_cycleTime,
                m_growthStartTime = dto.m_plant.m_growthStartTime,
            });
            if (state != PlantState.Harvestable)
            {
                // todo: we could handle perished here, just give 0 reward...
                throw new Exception($"attempt to harvest a plant in the wrong state: {state}");
            }
            
            if (dto.m_plant.m_category == SeedCategory.Perishable)
            {
                var rowsDeleted = await m_dbContext.m_nestGardenSeeds
                    .Where(x => x.m_id == request.m_plantID)
                    .ExecuteDeleteAsync();
                if (rowsDeleted == 0)
                {
                    throw new InvalidDataException("plant to harvest doesn't exist (race)");
                }
            } else
            {
                var newGrowthStart = m_timeProvider.GetUtcNow() - TimeSpan.FromMinutes(dto.m_plant.m_growTime);
                
                var rowsUpdated = await m_dbContext.m_nestPlacedSeeds
                    .Where(x => x.m_id == request.m_plantID)
                    .Where(x => x.m_growthStartTime == dto.m_plant.m_growthStartTime) // concurrency check
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.m_growthStartTime, newGrowthStart));
                if (rowsUpdated == 0)
                {
                    throw new InvalidDataException("plant to harvest doesn't exist (race)");
                }
            }
            
            var weevRowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + dto.m_plant.m_mulchYield)
                    .SetProperty(x => x.m_xp, x => x.m_xp + dto.m_plant.m_xpYield));
            if (weevRowsUpdated == 0)
            {
                throw new Exception("unable to add harvest rewards");
            }
            
            // avoid concurrency check, not needed
            // todo: the client won't reload other's plant configs so...
            /*await m_dbContext.m_nests
                .Where(x => x.m_id == nestID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lastUpdated, DateTime.UtcNow));*/
            
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
        
        [StructuredFormPost("remove-plant")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task RemovePlant([FromBody] HarvestPlantRequest request)
        {
            var validCheck = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(weev => weev.m_nest.m_gardenSeeds)
                .Where(x => x.m_id == request.m_plantID)
                .Where(x => x.m_placedItem != null)
                .AnyAsync();
            if (!validCheck)
            {
                throw new Exception("invalid remove plant request");
            }
            
            await m_dbContext.m_nestGardenSeeds
                .Where(x => x.m_id == request.m_plantID)
                .ExecuteDeleteAsync();
        }
        
        private struct PlantStateData
        {
            public required byte m_weevilHappiness;
            
            public required SeedCategory m_category;
            public required uint m_growTime;
            public required uint m_cycleTime;
            public required DateTimeOffset m_growthStartTime;
        }
        
        private enum PlantState
        {
            Growing,
            Harvestable,
            Perished
        }
        
        private uint GetPlantAge(DateTimeOffset growthStartTime)
        {
            return GetPlantAge(m_timeProvider, growthStartTime);
        }
        
        private static uint GetPlantAge(TimeProvider timeProvider, DateTimeOffset growthStartTime)
        {
            // static to help efcore with client eval
            
            var timeDelta = timeProvider.GetUtcNow() - growthStartTime;
            var age = timeDelta.Minutes;
            return (uint)age;
        }
        
        private PlantState GetPlantState(PlantStateData data) 
        {
            var age = GetPlantAge(data.m_growthStartTime);
            
            // note: we have to do an approx check on the age for harvesting...
            // the client has a global update that runs once a minute
            // which can cause a plant to age instantly after planting...
            switch (data.m_category)
            {
                case SeedCategory.Perishable:
                {
                    // does not care about weevil happiness
                    // todo: why does the client offset by -2?
                    // i would guess its to combat timer rounding
                    var perishAge = data.m_growTime + data.m_cycleTime;
                    if (age >= perishAge)
                    {
                        return PlantState.Perished;
                    }
                
                    // note: rounded
                    var growTime = (int)(data.m_growTime * 250 / (double)(150 + data.m_weevilHappiness));
                    if (age+1 >= growTime)
                    {
                        return PlantState.Harvestable;
                    }
                    return PlantState.Growing;
                }
                case SeedCategory.Reharvest:
                {
                    // does not care about weevil happiness
                    var growTime = data.m_growTime;
                    if (age < growTime) 
                    {
                        return PlantState.Growing;
                    }
                
                    // note: not rounded
                    var fruitTime = growTime + data.m_cycleTime * 150 / (double)(50 + data.m_weevilHappiness);
                    if (age+1 >= fruitTime)
                    {
                        return PlantState.Harvestable;
                    }
                    return PlantState.Growing; // fruiting
                }
                default:
                {
                    throw new InvalidDataException($"unknown plant type: {data.m_category}");
                }
            }
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
            using var activity = ApiServerObservability.StartActivity("GardenController.ValidatePlacement");

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