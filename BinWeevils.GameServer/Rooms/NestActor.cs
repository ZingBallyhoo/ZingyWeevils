using ArcticFox.SmartFoxServer;
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

            await request.user.MoveTo(request.room);
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
            
            m_invitedUsers.Add(name, weevilData.m_userActor);
            context.Watch(weevilData.m_userActor);
                    
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
            
            var otherUser = await m_us.m_zone.GetUser(name);
            if (otherUser == null) return;
            
            await otherUser.BroadcastXtStr(Modules.NEST_DENY_NEST_INVITE, new NestInvite
            {
                m_userName = m_us.m_name
            });
            await otherUser.BroadcastXtStr(Modules.NEST_RETURN_TO_NEST, new NestInvite
            {
                m_userName = m_us.m_name
            });
            
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
    }
    
    public class NestRoom : StatelessRoom
    {
        public PID m_nest;
    }
}