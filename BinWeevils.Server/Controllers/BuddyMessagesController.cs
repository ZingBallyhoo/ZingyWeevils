using System.Net.Mime;
using System.Text.RegularExpressions;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form.BuddyMessage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public partial class BuddyMessagesController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly TimeProvider m_timeProvider;
        
        [GeneratedRegex(@"^[a-zA-Z?!\-.&]{1,100}$")]
        private partial Regex MessageRegex { get; }
        
        public BuddyMessagesController(WeevilDBContext dbContext, TimeProvider timeProvider)
        {
            m_dbContext = dbContext;
            m_timeProvider = timeProvider;
        }
        
        [StructuredFormPost("buddy-messages/send-buddy-message")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task SendBuddyMessage([FromBody] SendBuddyMessageRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("BuddyMessagesController.SendBuddyMessage");
            activity?.SetTag("recipientIdx", request.m_recipientIdx);
            activity?.SetTag("messageLength", request.m_message.Length);
            
            if (request.m_message.Length <= 0)
            {
                throw new InvalidDataException($"buddy message too short ({request.m_message.Length})");
            }
            if (request.m_message.Length > 100)
            {
                throw new InvalidDataException($"buddy message too long ({request.m_message.Length})");
            }
            if (!MessageRegex.IsMatch(request.m_message))
            {
                throw new InvalidDataException("buddy message failed character validation");
            }
            
            var expectedHash = Rssmv.Hash($"{request.m_message}{request.m_recipientIdx}");
            if (expectedHash != request.m_hash)
            {
                throw new InvalidDataException("invalid send buddy message hash");
            }
            
            var checkDto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(weev => m_dbContext.m_buddyRecords.Any(x => 
                    x.m_weevil1ID == weev.m_idx && x.m_weevil2ID == request.m_recipientIdx ||
                    x.m_weevil2ID == weev.m_idx && x.m_weevil1ID == request.m_recipientIdx))
                .Select(x => new
                {
                    x.m_idx
                })
                .SingleOrDefaultAsync();
            if (checkDto == null)
            {
                throw new InvalidDataException("can't send buddy message - not buddies");
            }
            
            await m_dbContext.m_buddyMesssages.AddAsync(new BuddyMessageDB
            {
                m_to = request.m_recipientIdx,
                m_from = checkDto.m_idx,
                m_message = request.m_message,
                m_sentAt = m_timeProvider.GetUtcNow(),
                m_read = false
            });
            await m_dbContext.SaveChangesAsync();
        }
        
        [HttpGet("buddy-messages/get-buddy-messages")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<Dictionary<string, string>> GetBuddyMessages()
        {
            return new Dictionary<string, string>
            {
                {"success", "true"},
                {"numMessages", "1"},
                
                {"from0", "h"},
                {"fromIDX0", "192170"},
                {"msg0", "hhhhhhhhhhhhhhhhhhh"},
                {"id0", "1"},
                {"read0", "0"},
                {"timeSent0", m_timeProvider.GetLocalNow().ToString("dd/MM")}
            };
        }
        
        // todo: mark-read
        // todo: delete
    }
}