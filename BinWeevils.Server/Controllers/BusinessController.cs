using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class BusinessController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public BusinessController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("php/buyTycoonBusinessPremises.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyPremisesResponse> BuyPremises([FromBody] BuyPremisesRequest request)
        {
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var locType = (ENestRoom)request.m_locTypeID;
            int cost;
            switch (locType)
            {
                case ENestRoom.PlazaLeft1:
                case ENestRoom.PlazaLeft2:
                case ENestRoom.PlazaRight1:
                case ENestRoom.PlazaRight2:
                case ENestRoom.PhotoStudio:
                {
                    cost = 5000;
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"invalid premises room: {locType}");
                }
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_mulch >= cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - cost));
            if (rowsUpdated == 0)
            {
                return new BuyPremisesResponse
                {
                    m_result = BuyPremisesResponse.RESULT_POOR
                };
            }
            
            var nest = 
                await m_dbContext.m_weevilDBs
                    .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                    .Select(x => x.m_nest)
                    .SingleAsync();
            
            var newRoom = new NestRoomDB
            {
                m_type = locType
            };
            nest.m_rooms.Add(newRoom);
            await m_dbContext.SaveChangesAsync();
            
            var result = new BuyPremisesResponse
            {
                m_result = BuyPremisesResponse.RESULT_SUCCESS,
                m_locID = newRoom.m_id,
                m_locTypeID = request.m_locTypeID
            };
            
            await transaction.CommitAsync();
            return result;
        }
    }
}