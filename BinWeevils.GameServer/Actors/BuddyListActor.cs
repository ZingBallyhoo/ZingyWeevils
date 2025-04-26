using System.Text.RegularExpressions;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.XmlMessages;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public partial class BuddyListActor : IActor
    {
        public required WeevilSocketServices m_services;
        public required User m_user;
        
        private readonly HashSet<string> m_buddies = [];
        private readonly HashSet<string> m_receivedBuddyRequests = [];
        
        public record LoadBuddyListRequest();
        private enum BuddyState
        {
            Add,
            Online,
            Update,
            Offline
        }
        private record BuddyUpdate(string name, BuddyState state);
        private record BuddyRemovedNotification(string name);
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                {
                    await HandleStart(context);
                    break;
                }
                case LoadBuddyListRequest:
                {
                    await HandleBuddyListRequest();
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
                    await HandleBuddyUpdate(context, buddyConfirmed.name, buddyConfirmed.state);
                    break;
                }
                case RemoveBuddyBody removeBuddyRequest:
                {
                    await HandleRemoveBuddyRequest(context, removeBuddyRequest);
                    break;
                }
                case BuddyRemovedNotification buddyRemoved:
                {
                    await HandleBuddyRemoved(context, buddyRemoved);
                    break;
                }
                case Terminated buddyTerminated:
                {
                    var match = BuddyListRegex.Match(buddyTerminated.Who.Id);
                    if (match.Success)
                    {
                        await HandleBuddyUpdate(context, match.Groups[1].Value, BuddyState.Offline);
                    }
                    break;
                }
                case FindBuddyRequest findBuddyRequest:
                {
                    await HandleFindBuddy(findBuddyRequest);
                    break;
                }
                case SetBuddyVarsRequest setBuddyVarsRequest:
                {
                    await HandleSetBuddyVars(context, setBuddyVarsRequest);
                    break;
                }
            }
        }
        
        [GeneratedRegex("^([^/]+)/buddyList$")]
        private static partial Regex BuddyListRegex { get; }
        
        private static PID GetBuddyListAddress(IContext context, string name)
        {
            return new PID(context.Self.Address, $"{name}/buddyList");
        }
        
        private async Task HandleStart(IContext context)
        {
            var idx = m_user.GetUserData<WeevilData>().m_idx.GetValue();
            
            await foreach (var buddyName in m_services.GetBuddies(idx))
            {
                m_buddies.Add(buddyName);
                var buddyUser = await m_user.m_zone.GetUser(buddyName);
                if (buddyUser == null)
                {
                    // don't watch offline buddies...
                    // we would instantly get notified
                    continue;
                }
                
                var theirBuddyList = GetBuddyListAddress(context, buddyName);
                context.Watch(theirBuddyList);
                context.Send(theirBuddyList, new BuddyUpdate(m_user.m_name, BuddyState.Online));
            }
        }
        
        private async Task HandleBuddyListRequest()
        {
            var buddyList = new BuddyList();
            
            foreach (var buddyName in m_buddies)
            {
                var buddyUser = await m_user.m_zone.GetUser(buddyName);
                buddyList.m_buddies.Add(CreateBuddyUpdateForUser(buddyUser, buddyName));
            }
            
            await m_user.BroadcastSys(new BuddyListResponse
            {
                m_action = "bList",
                m_list = buddyList
            });
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
            
            if (m_receivedBuddyRequests.Contains(request.m_targetName))
            {
                await ConfirmAddBuddy(context, request.m_targetName);
                return;
            }
            
            var theirBuddyList = GetBuddyListAddress(context, request.m_targetName);
            context.Send(theirBuddyList, new BuddyPermissionRequest
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
            
            // we don't need to eagerly confirm here, let the client do it instead
            // otherwise we have to track denying requests... not great
            
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
            
            var theirBuddyList = GetBuddyListAddress(context, otherWeevilName);
            context.Send(context.Self, new BuddyUpdate(otherWeevilName, BuddyState.Add));
            context.Send(theirBuddyList, new BuddyUpdate(m_user.m_name, BuddyState.Add));
        }
        
        private async Task HandleBuddyUpdate(IContext context, string buddyUserName, BuddyState userState)
        {
            if (userState == BuddyState.Add)
            {
                m_receivedBuddyRequests.Remove(buddyUserName);
                if (!m_buddies.Add(buddyUserName))
                {
                    return;
                }
            } else if (!m_buddies.Contains(buddyUserName))
            {
                return;
            }
            
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
                var theirBuddyList = GetBuddyListAddress(context, buddyUserName);
                context.Watch(theirBuddyList);
            }
        }
        
        private static BuddyUpdateRecord CreateBuddyUpdateForUser(User? user, string name)
        {
            var varList = new BuddyUpdateRecord.VarList();
            
            var weevilData = user?.GetUserDataAs<WeevilData>();
            if (weevilData != null)
            {
                varList.m_buddyVars.Add(new BuddyUpdateRecord.Var
                {
                    m_name = "locName",
                    m_value = weevilData.m_buddyLocName
                });
            }
            
            return new BuddyUpdateRecord
            {
                m_name = name,
                m_isBlocked = false,
                m_isOnline = user != null,
                m_userID = user != null ? checked((int)user.m_id) : -1,
                m_varList = varList
            };
        }
        
        private async Task HandleRemoveBuddyRequest(IContext context, RemoveBuddyBody request)
        {
            if (!m_buddies.Remove(request.m_buddyName))
            {
                return;
            }
            
            if (!await m_services.RemoveBuddy(m_user.m_name, request.m_buddyName))
            {
                // lost a race
                return;
            }
            
            var theirBuddyList = GetBuddyListAddress(context, request.m_buddyName);
            context.Unwatch(theirBuddyList);
            context.Send(theirBuddyList, new BuddyRemovedNotification(m_user.m_name));
        }
        
        private async Task HandleBuddyRemoved(IContext context, BuddyRemovedNotification buddyRemoved)
        {
            var buddyName = buddyRemoved.name;
            if (!m_buddies.Remove(buddyName))
            {
                return;
            }
            
            await m_user.BroadcastSys(new RemoveBuddyBody
            {
                m_action = "remB",
                m_buddyName = buddyName
            });
            
            var theirBuddyList = GetBuddyListAddress(context, buddyName);
            context.Unwatch(theirBuddyList);
        }
        
        private async Task HandleFindBuddy(FindBuddyRequest findBuddyRequest)
        {
            var buddyUser = await m_user.m_zone.GetUser(findBuddyRequest.m_record.m_id);
            if (buddyUser == null)
            {
                return;
            }
            if (!m_buddies.Contains(buddyUser.m_name))
            {
                return;
            }
                    
            var buddyRoom = await buddyUser.GetRoomOrNull();
            await m_user.BroadcastSys(new BuddyRoomResponse
            {
                m_action = "roomB",
                m_list = new BuddyRoomList
                {
                    m_rooms = buddyRoom != null ? [buddyRoom.m_id] : []
                }
            });
        }
        
        private async Task HandleSetBuddyVars(IContext context, SetBuddyVarsRequest request)
        {
            var locNameVar = request.m_varList.m_buddyVars.SingleOrDefault(x => x.m_name == "locName");
            if (locNameVar.m_value == null) return;
            
            var weevilData = m_user.GetUserData<WeevilData>();
            var mappedName = await m_services.GetLocNameMapper().MapName(locNameVar.m_value);
            if (mappedName == weevilData.m_buddyLocName)
            {
                return;
            }
            
            weevilData.m_buddyLocName = mappedName;
            foreach (var buddyName in m_buddies)
            {
                var theirBuddyList = GetBuddyListAddress(context, buddyName);
                context.Send(theirBuddyList, new BuddyUpdate(m_user.m_name, BuddyState.Update));
            }
        }
    }
}