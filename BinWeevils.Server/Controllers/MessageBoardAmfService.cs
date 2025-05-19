using ArcticFox.PolyType.Amf;
using ArcticFox.RPC.AmfGateway;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Amf;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    public class MessageBoardAmfService
    {
        private readonly WeevilDBContext m_dbContext;
        
        public MessageBoardAmfService(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        public Task<MessageBoardResponse> GetExistingTopics(AmfGatewayContext context)
        {
            using var activity = ApiServerObservability.StartActivity("MessageBoardAmfService.GetWeevilDefs");

            var result = new MessageBoardResponse
            {
                m_resultObject = new MessageBoardResultObject
                {
                    m_serverInfo = new MessageBoardServerInfo
                    {
                        m_initialData = [
                            (object?[])ArrayMapper.ToArray(new MessageBoardTopic
                            {
                                m_topicID = 99,
                                m_weevilID = context.m_httpContext.User.Identity!.Name!,
                                m_boardID = 88,
                                m_title = "Message Board Demo",
                                m_message = "This is not a real post, just a demo of the message board system",
                                m_dateStarted = "today",
                                m_replies = 0,
                                m_views = 0,
                                m_active = 0,
                                m_sticky = true,
                                m_lastReply = "now",
                                m_closed = false,
                            })!,
                        ]
                    }
                },
                m_numRows = 1,
            };
            return Task.FromResult(result);
        }
        
        public async Task<MessageBoardResultObject> GetWeevilDefs(AmfGatewayContext context, MessageBoardGetWeevilDefsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("MessageBoardAmfService.GetWeevilDefs");
        
            // todo: polytype my beloved
            var rows = new List<object?[]>();
            foreach (var userName in request.m_delimitedUserNames.Split(','))
            {
                var weevil = await m_dbContext.m_weevilDBs
                    .Where(x => x.m_name == userName)
                    .Select(x => new 
                    {
                        x.m_weevilDef,
                        x.m_lastAcknowledgedLevel
                    })
                    .SingleAsync();
                
                rows.Add([userName, $"{weevil.m_weevilDef}", weevil.m_lastAcknowledgedLevel, 0]);
            }
            
            return new MessageBoardResultObject
            {
                m_serverInfo = new MessageBoardServerInfo
                {
                    m_columnNames = ["userWeevilID", "weevilDef", "experience", "king"],
                    m_initialData = rows.ToArray()
                }
            };
        }
    }
}