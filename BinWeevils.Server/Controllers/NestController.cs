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
    [Route("api/nest")]
    public class NestController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public NestController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [HttpGet("get-weevil-stats")]
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
        
        [HttpGet("level-up")]
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
        
        [StructuredFormPost("get-stored-items")]
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
            
            var h = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name ==  ControllerContext.HttpContext.User.Identity!.Name)
                .SelectMany(x => x.m_nest.m_items)
                .Where(x => x.m_placedItem == null)
                .Select(x => new
                {
                    x.m_id,
                    m_configName = x.m_itemType.m_configLocation
                })
                .ToArrayAsync();
            
            var storedItems = new StoredItems();
            foreach (var h2 in h)
            {
                storedItems.m_items.Add(new NestInventoryItem
                {
                    m_id = h2.m_id,
                    m_configName = h2.m_configName,
                    m_deliveryTime = 0,
                    m_clrTemp = ""
                });
            }
            
            return storedItems;
        }
    }
}