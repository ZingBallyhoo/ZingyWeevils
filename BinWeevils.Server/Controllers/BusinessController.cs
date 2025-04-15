using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form.Business;
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
            EBusinessType businessType;
            switch (locType)
            {
                case ENestRoom.PlazaLeft1:
                case ENestRoom.PlazaLeft2:
                case ENestRoom.PlazaRight1:
                case ENestRoom.PlazaRight2:
                {
                    cost = 5000;
                    businessType = EBusinessType.NightClub;
                    break;
                }
                case ENestRoom.PhotoStudio:
                {
                    cost = 5000;
                    businessType = EBusinessType.PhotoStudio;
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
                m_type = locType,
                m_business = new BusinessDB
                {
                    m_type = businessType,
                    m_name = "",
                    m_open = false
                }
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
        
        [StructuredFormPost("php/submitTycoonBusinessName.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SubmitBusinessNameResponse> SubmitBusinessName([FromBody] SubmitBusinessNameRequest request)
        {
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var nest = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(weev => weev.m_nest.m_rooms.Any(x => x.m_id == request.m_locID))
                .Select(weev => weev.m_nest)
                .SingleAsync();
            if (nest == null)
            {
                throw new Exception("invalid change name request");
            }
            
            var rowsUpdated = await m_dbContext.m_businesses
                .Where(x => x.m_id == request.m_locID)
                .Where(x => x.m_type == EBusinessType.NightClub)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_name, request.m_name));
            if (rowsUpdated == 0)
            {
                throw new Exception("business name change failed");
            }
            
            nest.m_lastUpdated = DateTime.Now;
            await m_dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            return new SubmitBusinessNameResponse
            {
                m_result = SubmitBusinessNameResponse.RESULT_SUCCESS
            };
        }
    }
}