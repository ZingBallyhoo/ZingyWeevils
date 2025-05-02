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
                m_sentAt = m_timeProvider.GetUtcNow().DateTime,
                m_read = false
            });
            await m_dbContext.SaveChangesAsync();
        }
        
        [HttpGet("php/hasUnreadBuddyMsg.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<HasUnreadBuddyMessageResponse> HasUnreadBuddyMessage()
        {
            using var activity = ApiServerObservability.StartActivity("BuddyMessagesController.HasUnreadBuddyMessage");
            
            var count = await m_dbContext.m_buddyMesssages
                .Where(x => x.m_toWeevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => !x.m_read)
                .CountAsync();
            
            // todo: is this a bool or count?
            // i'm assuming count because the code checks for >0
            return new HasUnreadBuddyMessageResponse
            {
                m_result = count
            };
        }
        
        [HttpGet("buddy-messages/get-buddy-messages")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<Dictionary<string, string>> GetBuddyMessages()
        {
            using var activity = ApiServerObservability.StartActivity("BuddyMessagesController.GetBuddyMessages");
            var dict = new Dictionary<string, string>();
            
            var messages = await m_dbContext.m_buddyMesssages
                .Where(x => x.m_toWeevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new 
                {
                    m_fromName = x.m_fromWeevil.m_name,
                    m_fromIdx = x.m_from,
                    x.m_message,
                    x.m_id,
                    x.m_read,
                    x.m_sentAt
                })
                .OrderByDescending(x => x.m_sentAt)
                .ToListAsync();
            
            var index = 0;
            foreach (var messageDto in messages) 
            {
                dict.Add($"from{index}", messageDto.m_fromName);
                dict.Add($"fromIDX{index}", $"{messageDto.m_fromIdx}");
                dict.Add($"msg{index}", messageDto.m_message);
                dict.Add($"id{index}", $"{messageDto.m_id}");
                dict.Add($"read{index}", $"{(messageDto.m_read ? "1" : "0")}");
                dict.Add($"timeSent{index}", TimeZoneInfo.ConvertTimeFromUtc(messageDto.m_sentAt, m_timeProvider.LocalTimeZone).ToString("dd/MM"));
                
                index++;
            }
            
            dict.Add("success", $"{(index != 0 ? "true" : "false")}");
            dict.Add("numMessages", $"{index}");
            return dict;
        }
        
        [StructuredFormPost("buddy-messages/mark-read")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task MarkRead([FromBody] MarkReadRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("BuddyMessagesController.MarkRead");
            activity?.SetTag("id", request.m_id);

            var rowsUpdated = await m_dbContext.m_buddyMesssages
                .Where(x => x.m_toWeevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_id == request.m_id)
                .Where(x => !x.m_read)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_read, true));
            
            if (rowsUpdated == 0)
            {
                throw new Exception("failed to mark read");
            }
        }
        
        // todo: delete
    }
}