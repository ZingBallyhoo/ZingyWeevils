using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Form.Weevil;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class WeevilController : Controller
    {
        private readonly ILogger<WeevilController> m_logger;
        private readonly WeevilDBContext m_dbContext;
        private readonly TimeProvider m_timeProvider;
        
        public WeevilController(ILogger<WeevilController> logger, WeevilDBContext dbContext, TimeProvider timeProvider)
        {
            m_logger = logger;
            m_dbContext = dbContext;
            m_timeProvider = timeProvider;
        }
        
        [StructuredFormPost("weevil/data")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<WeevilDataResponse> GetData([FromBody] WeevilDataRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetData");
            activity?.SetTag("name", request.m_name);
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_name)
                .Select(x => new
                {
                    x.m_idx,
                    x.m_createdAt,
                    x.m_lastLogin,
                    x.m_weevilDef,
                    x.m_lastAcknowledgedLevel
                })
                .SingleOrDefaultAsync();
            if (dto == null)
            {
                m_logger.LogWarning("Tried to get data for unknown weevil: {Name}", request.m_name);
                return new WeevilDataResponse
                {
                    m_result = 0
                };
            }
            
            return new WeevilDataResponse
            {
                m_result = 1,
                m_idx = dto.m_idx,
                m_weevilDef = dto.m_weevilDef,
                m_level = dto.m_lastAcknowledgedLevel,
                m_tycoon = 1,
                m_lastLog = TimeZoneInfo.ConvertTimeFromUtc(dto.m_lastLogin, m_timeProvider.LocalTimeZone).ToAs3Date(),
                m_dateJoined = TimeZoneInfo.ConvertTimeFromUtc(dto.m_createdAt, m_timeProvider.LocalTimeZone).ToAs3Date()
            };
        }
        
        [StructuredFormPost("php/getWeevilDefinition.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetWeevilDefResponse> GetWeevilDef([FromBody] GetWeevilDefRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetWeevilDef");
            activity?.SetTag("userID", request.m_userID);
            
            var weevilDef = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .Select(x => x.m_weevilDef)
                .SingleAsync();
            
            return new GetWeevilDefResponse
            {
                m_weevilDef = weevilDef
            };
        }
        
        [StructuredFormPost("weevil/buy-food")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyFoodResponse> BuyFood([FromBody] BuyFoodRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.BuyFood");
            activity?.SetTag("cost", request.m_cost);
            activity?.SetTag("energyValue", request.m_energyValue);
            
            if (request.m_cost > 25 || request.m_energyValue > 45) 
            {
                throw new InvalidDataException("food value out of range");
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= request.m_cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_food, x => Math.Min(x.m_food + request.m_energyValue, 100))
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - request.m_cost));
            if (rowsUpdated == 0)
            {
                m_logger.LogError("Not enough money to buy food! Cost: {Cost}", request.m_cost);
                return new BuyFoodResponse
                {
                    m_success = 0
                };
            }
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_food,
                    x.m_mulch
                })
                .SingleAsync();
            
            ApiServerObservability.s_foodItemsBought.Add(1);
            ApiServerObservability.s_foodItemsCost.Add(request.m_cost);
            ApiServerObservability.s_foodBought.Add(request.m_energyValue);
            
            return new BuyFoodResponse
            {
                m_success = 1,
                m_food = dto.m_food,
                m_mulch = dto.m_mulch
            };
        }
        
        [StructuredFormPost("weevil/update-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<UpdateStatsResponse> UpdateStats([FromBody] UpdateStatsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.UpdateStats");

            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_food >= request.m_food) // can only decrease
                .Where(x => Math.Abs(x.m_fitness - request.m_fitness) <= 20)
                .Where(x => Math.Abs(x.m_happiness - request.m_happiness) <= 10)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_food, Math.Min(request.m_food, (byte)100))
                    .SetProperty(x => x.m_fitness, Math.Min(request.m_fitness, (byte)100))
                    .SetProperty(x => x.m_happiness, Math.Min(request.m_happiness, (byte)100)));
            return new UpdateStatsResponse
            {
                m_result = rowsUpdated
            };
        }
        
        [HttpGet("weevil/get-progress")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetIntroProgressResponse> GetIntroProgress()
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetIntroProgress");

            var progress = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_introProgress)
                .SingleAsync();
            
            var encodedProgress = new EncodedIntroProgress(progress);
            return new GetIntroProgressResponse
            {
                m_result = encodedProgress.ToString()
            };
        }
        
        [StructuredFormPost("weevil/set-progress")]
        public async Task SetIntroProgress([FromBody] SetIntroProgressRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.SetIntroProgress");
            activity?.SetTag("progress", request.m_progress);

            var encodedProgress = new EncodedIntroProgress(request.m_progress);
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => (x.m_introProgress & encodedProgress.m_bits) == x.m_introProgress) // dont progress backwards
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_introProgress, encodedProgress.m_bits));
            
            if (rowsUpdated == 0)
            {
                throw new InvalidDataException("tried remove intro progress bits?");
            }
        }
        
        [StructuredFormPost("weevil/change-definition")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<ChangeWeevilDefResponse> ChangeDefinition([FromBody] ChangeWeevilDefRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.ChangeDefinition");
            activity?.SetTag("weevilDef", request.m_weevilDef);

            var def = new WeevilDef($"{request.m_weevilDef}");
            if (!def.ValidateLegacy())
            {
                throw new InvalidDataException($"ChangeDefinition: invalid weevil def: \"{request.m_weevilDef}\"");
            }
            if (def.HasSuperAntenna())
            {
                throw new InvalidDataException("not allowed to keep/add super antenna when changing def");
            }
            
            const int cost = 2500;
            var defNum = def.AsNumber();
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - cost)
                    .SetProperty(x => x.m_weevilDef, defNum));
            
            if (rowsUpdated == 0)
            {
                return new ChangeWeevilDefResponse
                {
                    m_error = ChangeWeevilDefResponse.ERR_NOT_ENOUGH_MONEY
                };
            }
            return new ChangeWeevilDefResponse
            {
                m_error = ChangeWeevilDefResponse.ERR_OK
            };
        }
        
        [HttpGet("weevil/get-my-apparel")]
        [Produces(MediaTypeNames.Application.Xml)]
        public async Task<OwnedApparelList> GetMyApparel()
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetMyApparel");
            
            var wornDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name) 
                .Select(x => new
                {
                    x.m_apparelTypeID,
                    x.m_apparelPaletteEntryIndex
                })
                .SingleAsync();
            
            var list = new OwnedApparelList();

            await foreach (var apparelType in m_dbContext.m_apparelTypes
                .OrderBy(x => x.m_id)
                .Select(x => new
                {
                    x.m_id,
                    x.m_category,
                    x.m_paletteID,
                    m_entries = m_dbContext.m_paletteEntries
                        .Where(y => x.m_paletteID == y.m_paletteID)
                        .OrderBy(y => y.m_index)
                        .Select(y => new
                        {
                            y.m_index,
                            y.m_colorString
                        })
                        .ToList()
                })
                .AsAsyncEnumerable())
            {
                foreach (var entry in apparelType.m_entries)
                {
                    var isWearingExact = 
                        wornDto.m_apparelTypeID == apparelType.m_id &&
                        wornDto.m_apparelPaletteEntryIndex == entry.m_index;
                    
                    list.m_items.Add(new OwnedApparelEntry
                    {
                        m_id = apparelType.m_id,
                        m_category = apparelType.m_category,
                        m_rgb = entry.m_colorString,
                        m_worn = isWearingExact,
                    });
                }
            }
            
            return list;
        }
        
        [StructuredFormPost("php/getSpecialMoves.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetSpecialMovesResponse> GetSpecialMoves([FromBody] GetSpecialMovesRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("WeevilController.GetSpecialMoves");
            
            if (request.m_userID != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to get someone else's special moves");
            }
            
            var specialMoves = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name) 
                .SelectMany(x => x.m_specialMoves)
                .OrderBy(x => x.m_action)
                .Select(x => x.m_action)
                .ToListAsync();
            
            return new GetSpecialMovesResponse
            {
                m_responseCode = 1,
                m_result = string.Join(';', specialMoves.Select(x => (int)x))
            };
        }
    }
}