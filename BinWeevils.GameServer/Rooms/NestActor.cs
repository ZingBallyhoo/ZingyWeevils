using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Actors;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using Proto;

namespace BinWeevils.GameServer.Rooms
{
    public class NestActor : IActor
    {
        public required User m_us;
        private readonly Dictionary<string, PID> m_invitedUsers = [];
        
        public record Join(User user, Room room);
        public record RemoveGuest(string userName);
        public record RemoveAllGuests();
        public record DenyNestInvite(string userName);
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Join join:
                {
                    var result = await TryJoin(join);
                    context.Respond(result);
                    break;
                }
                case NestInvite invite:
                {
                    await AddInvite(context, invite.m_userName);
                    break;
                }
                case DenyNestInvite denyNestInvite:
                {
                    await GuestRemovedInvite(context, denyNestInvite.userName);
                    break;
                }
                case Terminated guestTerminated:
                {
                    await GuestRemovedInvite(context, guestTerminated.Who.Id);
                    break;
                }
                case RemoveGuest removeNestInvite:
                {
                    await KickGuest(context, removeNestInvite.userName);
                    break;
                }
                case RemoveAllGuests:
                case Stopping:
                {
                    await KickAllGuests(context);
                    break;
                }
                case Restarting:
                {
                    await HandleRestarting(context);
                    break;
                }
            }
        }
        
        private async Task<bool> TryJoin(Join request)
        {
            if (m_us.m_name != request.user.m_name && !m_invitedUsers.ContainsKey(request.user.m_name))
            {
                // try to salvage the situation...
                await request.user.BroadcastXtStr(Modules.NEST_RETURN_TO_NEST, new NestInvite
                {
                    m_userName = m_us.m_name
                });
                return false;
            }

            try
            {
                await request.user.MoveTo(request.room);
            } catch (Exception e)
            {
                // todo: log
                return false;
            }
            return true;
        }
        
        private async Task AddInvite(IContext context, string name)
        {
            if (m_invitedUsers.ContainsKey(name)) return;
            
            var otherUser = await m_us.m_zone.GetUser(name);
            if (otherUser?.GetUserDataAs<WeevilData>() is not {} weevilData)
            {
                // oops.. remove the invite from our ui
                await m_us.BroadcastXtStr(Modules.NEST_REMOVE_GUESTS, new NestInvite
                {
                    m_userName = name
                });
                return;
            }
            
            m_invitedUsers.Add(name, weevilData.GetUserAddress());
            context.Watch(weevilData.GetUserAddress());
                    
            await otherUser.BroadcastXtStr(Modules.NEST_INVITE_TO_NEST, new NestInvite
            {
                m_userName = m_us.m_name
            });
        }
        
        private async Task GuestRemovedInvite(IContext context, string name)
        {
            if (!m_invitedUsers.Remove(name, out var userActor)) return;
            
            await m_us.BroadcastXtStr(Modules.NEST_REMOVE_GUESTS, new NestInvite
            {
                m_userName = name
            });
            context.Unwatch(userActor);
        }
        
        private async Task KickGuest(IContext context, string name)
        {
            if (!m_invitedUsers.Remove(name, out var userActor))
            {
                return;
            }
            context.Unwatch(userActor);
            
            context.Send(userActor, new SocketActor.KickFromNest(m_us.m_name));
            
            // todo: remove from room..?
        }
        
        private async Task KickAllGuests(IContext context)
        {
            foreach (var userName in m_invitedUsers.Keys.ToArray())
            {
                await KickGuest(context, userName);
            }
            m_invitedUsers.Clear();
        }
        
        private async Task HandleRestarting(IContext context)
        {
            // clear our guest list
            // and remove invites from others
            foreach (var pair in m_invitedUsers)
            {
                await m_us.BroadcastXtStr(Modules.NEST_REMOVE_GUESTS, new NestInvite
                {
                    m_userName = pair.Key
                });
                context.Send(pair.Value, new SocketActor.KickFromNest(m_us.m_name));
            }
            m_invitedUsers.Clear();
        }
    }
    
    public class NestRoom : IStatefulRoom, IRoomEventHandler
    {
        public required PID m_nest;
        public required User m_owner;
        private string m_ownerName => m_owner.m_name;

        public async ValueTask UserJoinedRoom(Room room, User user)
        {
            if (user.m_name == m_ownerName) return;

            await m_owner.BroadcastXtStr(Modules.NEST_GUEST_JOINED_NEST, new NestGuestJoined
            {
                m_name = user.m_name,
                m_joined = 1
            });
        }

        public async ValueTask UserLeftRoom(Room room, User user)
        {
            if (user.m_name == m_ownerName) return;

            await m_owner.BroadcastXtStr(Modules.NEST_GUEST_JOINED_NEST, new NestGuestJoined
            {
                m_name = user.m_name,
                m_joined = 0
            });
        }

        public ValueTask<VarBag> GetVars()
        {
            // todo
            return ValueTask.FromResult(new VarBag());
        }
    }
}