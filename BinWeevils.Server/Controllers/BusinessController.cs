using System.Net.Mime;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form.Business;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server.Services;
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
        private readonly ILogger<BusinessController> m_logger;
        private readonly WeevilDBContext m_dbContext;
        private readonly TrackRepository m_trackRepository;
        
        private static readonly HashSet<int> s_allowedColors = [
            0x990000, 0x00AA00, 0x000099, 
            0x997700, 0x880088, 0xAADFFF, 
            0x0066FF, 0xFF9900, 0xCCCC00,
            0x00EEEE, 0xCC00CC, 0xFFFFFF,
            0xFFD5DD, 0xAAFF00, 0xFFCC00, 
            0xEEEE00, 0xFF8484, 0x282828, 
            0x999999, 0xFFFFB9, 0xEE0000, 
            0x006600,
            // the client sets the default text color to pitch black even though it isn't selectable
            0x000000
        ];
        
        public BusinessController(ILogger<BusinessController> logger, WeevilDBContext dbContext, 
            TrackRepository trackRepository)
        {
            m_logger = logger;
            m_dbContext = dbContext;
            m_trackRepository = trackRepository;
        }
        
        private async Task<uint> ValidateNest(uint locID)
        {
            var checkDto = await m_dbContext.m_weevilDBs
                .Where(weev => weev.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(weev => weev.m_nest.m_rooms.Any(x => x.m_id == locID))
                .Select(weev => new
                {
                    m_nestID = weev.m_nest.m_id
                })
                .SingleAsync();
            if (checkDto == null)
            {
                throw new Exception("invalid request - not our room");
            }
            return checkDto.m_nestID;
        }
        
        [StructuredFormPost("php/buyTycoonBusinessPremises.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<BuyPremisesResponse> BuyPremises([FromBody] BuyPremisesRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("BusinessController.BuyPremises");
            activity?.SetTag("locTypeID", request.m_locTypeID);
            
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
                m_logger.LogError("Not enouch mulch to buy {BusinessType} (cost: {Cost})", businessType, cost);
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
            using var activity = ApiServerObservability.StartActivity("SubmitBusinessName");
            activity?.SetTag("locID", request.m_locID);
            activity?.SetTag("name", request.m_name);
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var nestID = await ValidateNest(request.m_locID);
            var rowsUpdated = await m_dbContext.m_businesses
                .Where(x => x.m_id == request.m_locID)
                .Where(x => x.m_type == EBusinessType.NightClub)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_name, request.m_name));
            if (rowsUpdated == 0)
            {
                throw new Exception("business name change failed");
            }
            
            await m_dbContext.SetNestUpdatedNoConcurrency(nestID);
            await transaction.CommitAsync();
            
            return new SubmitBusinessNameResponse
            {
                m_result = SubmitBusinessNameResponse.RESULT_SUCCESS
            };
        }
        
        [StructuredFormPost("php/saveTycoonBusinessState.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task SaveBusinessState([FromBody] SaveBusinessStateRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("BusinessController.SaveBusinessState");
            activity?.SetTag("locID", request.m_locID);
            activity?.SetTag("signColor", request.m_signColor);
            activity?.SetTag("signTextColor", request.m_signTextColor);
            activity?.SetTag("busOpen", request.m_busOpen);
            
            if (!s_allowedColors.Contains(request.m_signColor) || !s_allowedColors.Contains(request.m_signTextColor))
            {
                throw new InvalidDataException("invalid color passed to saveTycoonBusinessState");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var nestID = await ValidateNest(request.m_locID);
            var rowsUpdated = await m_dbContext.m_businesses
                .Where(x => x.m_id == request.m_locID)
                .Where(x => x.m_type == EBusinessType.NightClub)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_signColor, request.m_signColor)
                    .SetProperty(x => x.m_signTextColor, request.m_signTextColor)
                    .SetProperty(x => x.m_open, request.m_busOpen));
            if (rowsUpdated == 0)
            {
                throw new Exception("business save state failed");
            }
            
            await m_dbContext.SetNestUpdatedNoConcurrency(nestID);
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("php/getPlaylistIDsForRoom.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetPlayListIDsForRoomResponse> GetPlayListIDsForRoom([FromBody] GetPlayListIDsForRoomRequest request)
        {
            // todo: only called by owner but doesn't really matter
            
            using var activity = ApiServerObservability.StartActivity("BusinessController.GetPlayListIDsForRoom");
            activity?.SetTag("locID", request.m_locID);
            
            var dto = await m_dbContext.m_businesses
                .Where(bus => bus.m_id == request.m_locID)
                .Select(x => new
                {
                    x.m_playList
                })
                .SingleOrDefaultAsync();
            
            if (dto == null)
            {
                return new GetPlayListIDsForRoomResponse
                {
                    m_success = false
                };
            }
            
            return new GetPlayListIDsForRoomResponse
            {
                m_success = true,
                m_playList = dto.m_playList.ToString()
            };
        }
        
        [StructuredFormPost("php/savePlayListIDsForRoom.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task SavePlayListIDsForRoom([FromBody] SavePlayListIDsForRoomRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("BusinessController.SavePlayListIDsForRoom");
            activity?.SetTag("locID", request.m_locID);
            activity?.SetTag("playListIDs", request.m_playListIDs);
            
            if (!PlayListIDs.TryParse(request.m_playListIDs, null, out var playList)) 
            {
                throw new InvalidDataException("invalid play list");
            }

            foreach (var value in playList.GetAllValues())
            {
                if (value == -1) continue;
                if (!m_trackRepository.IsBinTune(value))
                {
                    throw new InvalidDataException("trying to use a non-bin-tune track");
                }
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var nestID = await ValidateNest(request.m_locID);
            var rowsUpdated = await m_dbContext.m_businesses
                .Where(x => x.m_id == request.m_locID)
                .Where(x => x.m_type == EBusinessType.NightClub)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_playList, playList));
            if (rowsUpdated == 0)
            {
                throw new Exception("save playlist failed");
            }
            
            await m_dbContext.SetNestUpdatedNoConcurrency(nestID);
            await transaction.CommitAsync();
        }
    }
}