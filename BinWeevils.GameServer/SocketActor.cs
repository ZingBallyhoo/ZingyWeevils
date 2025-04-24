using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.XmlMessages;
using Proto;

namespace BinWeevils.GameServer
{
    public class SocketActor : IActor
    {
        public required WeevilSocketServices m_services;
        public required User m_user;
        private readonly HashSet<string> m_buddies = [];
        private readonly HashSet<string> m_sentBuddyRequests = [];
        private readonly HashSet<string> m_receivedBuddyRequests = [];
        
        public record CreateNest();
        public record KickFromNest(string userName);
        
        private enum BuddyState
        {
            Add,
            Online,
            Offline
        }
        private record BuddyUpdate(PID pid, BuddyState state);
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case CreateNest:
                {
                    await CreateNestNow(context);
                    break;
                }
                case KickFromNest kickFromNest:
                {
                    if (kickFromNest.userName == m_user.m_name)
                    {
                        // something has gone horribly wrong
                        context.Stop(context.Self);
                        return;
                    }
                    
                    await m_user.BroadcastXtStr(Modules.NEST_DENY_NEST_INVITE, new NestInvite
                    {
                        m_userName = kickFromNest.userName
                    });
                    await m_user.BroadcastXtStr(Modules.NEST_RETURN_TO_NEST, new NestInvite
                    {
                        m_userName = kickFromNest.userName
                    });
                    context.Respond(null!);
                    break;
                }
                case AddBuddyRequest addBuddy:
                {
                    await HandleAddBuddyRequest(context, addBuddy);
                    break;
                }
                case BuddyPermissionRequest buddyPermissionRequest:
                {
                    await HandleBuddyPermissionRequest(context, buddyPermissionRequest);
                    break;
                }
                case BuddyPermissionResponse buddyPermissionResponse:
                {
                    await HandleBuddyPermissionResponse(context, buddyPermissionResponse);
                    break;
                }
                case BuddyUpdate buddyConfirmed:
                {
                    await HandleBuddyUpdate(context, buddyConfirmed.pid, buddyConfirmed.state);
                    break;
                }
                case Terminated buddyTerminated:
                {
                    if (m_buddies.Contains(buddyTerminated.Who.Id))
                    {
                        await HandleBuddyUpdate(context, buddyTerminated.Who, BuddyState.Offline);
                    }
                    break;
                }
                case Stopping:
                {
                    m_user.m_socket?.Close();
                    break;
                }
            }
        }
        
        private async Task CreateNestNow(IContext context)
        {
            var nestProps = Props.FromProducer(() => new NestActor
            {
                m_us = m_user
            });
            var nestActor = context.SpawnNamed(nestProps, $"nest/{m_user.m_name}");
            
            var nestDesc = new WeevilRoomDescription($"nest_{m_user.m_name}")
            {
                m_creator = m_user,
                m_maxUsers = 20,
                m_isTemporary = true,
                m_data = new NestRoom
                {
                    m_ownerName = m_user.m_name,
                    m_nest = nestActor
                }
            };
            await m_user.m_zone.CreateRoom(nestDesc);
            
            context.Respond(nestActor);
        }
        
        private async Task HandleAddBuddyRequest(IContext context, AddBuddyRequest request)
        {
            if (request.m_targetName == m_user.m_name)
            {
                throw new InvalidDataException("tried to add self as buddy");
            }
            
            if (m_buddies.Contains(request.m_targetName))
            {
                return;
            }
            
            var otherUser = await m_user.m_zone.GetUser(request.m_targetName);
            var weevilData = otherUser?.GetUserDataAs<WeevilData>();
            if (weevilData == null) return;
            
            m_sentBuddyRequests.Add(request.m_targetName);
            // note: we don't want to check the result of this...
            // the other user can deny our request and we can try again in the future
            
            if (m_receivedBuddyRequests.Contains(request.m_targetName))
            {
                await ConfirmAddBuddy(context, request.m_targetName);
                return;
            }
            
            context.Send(weevilData.m_userActor, new BuddyPermissionRequest
            {
                m_action = "bPrm",
                m_sender = m_user.m_name
            });
        }
        
        private async Task HandleBuddyPermissionRequest(IContext context, BuddyPermissionRequest request)
        {
            if (m_buddies.Contains(request.m_sender))
            {
                return;
            }
            
            if (m_sentBuddyRequests.Contains(request.m_sender))
            {
                await ConfirmAddBuddy(context, request.m_sender);
                return;
            }
            if (!m_receivedBuddyRequests.Add(request.m_sender))
            {
                return;
            }
            
            await m_user.BroadcastSys(request);
        }
        
        private async Task HandleBuddyPermissionResponse(IContext context, BuddyPermissionResponse response)
        {
            if (m_buddies.Contains(response.m_inner.m_name))
            {
                return;
            }
            
            if (!m_receivedBuddyRequests.Contains(response.m_inner.m_name))
            {
                return;
            }
            
            if (response.m_inner.m_result != "g")
            {
                // said no...
                m_receivedBuddyRequests.Remove(response.m_inner.m_name);
                return;
            }
            
            await ConfirmAddBuddy(context, response.m_inner.m_name);
        }
        
        private async Task ConfirmAddBuddy(IContext context, string otherWeevilName)
        {
            if (!await m_services.AddBuddy(m_user.m_name, otherWeevilName))
            {
                // lost a race
                return;
            }
            
            var buddyAddress = PID.FromAddress(context.Self.Address, otherWeevilName);
            context.Send(context.Self, new BuddyUpdate(buddyAddress, BuddyState.Add));
            context.Send(buddyAddress, new BuddyUpdate(context.Self, BuddyState.Add));
        }
        
        private async Task HandleBuddyUpdate(IContext context, PID buddyPid, BuddyState userState)
        {
            var buddyUserName = buddyPid.Id;
            User? buddyUser = null;
            if (userState != BuddyState.Offline)
            {
                buddyUser = await m_user.m_zone.GetUser(buddyUserName);
            }
            
            await m_user.BroadcastSys(new BuddyUpdateNotification
            {
                m_action = userState == BuddyState.Add ? "bAdd" : "bUpd",
                m_record = CreateBuddyUpdateForUser(buddyUser, buddyUserName)
            });
            
            if (buddyUser != null)
            {
                // watch for log out
                context.Watch(buddyPid);
            }
            
            if (userState == BuddyState.Add)
            {
                m_buddies.Add(buddyUserName);
                m_receivedBuddyRequests.Remove(buddyUserName);
                m_sentBuddyRequests.Remove(buddyUserName);
            }
        }
        
        private static BuddyUpdateRecord CreateBuddyUpdateForUser(User? user, string name)
        {
            return new BuddyUpdateRecord
            {
                m_name = name,
                m_isBlocked = false,
                m_isOnline = user != null,
                m_userID = user != null ? checked((int)user.m_id) : -1,
                m_varList = new BuddyVarList()
            };
        }
    }
}