using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using Proto;

namespace BinWeevils.GameServer.Rooms
{
    public class NestActor : IActor
    {
        public required User m_us;
        private readonly HashSet<string> m_invitedUsers = [];
        
        public record Join(User user, Room room);
        public record RemoveNestGuest(string userName);
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
                    await AddInvite(invite.m_userName);
                    break;
                }
                case RemoveNestGuest removeNestInvite:
                {
                    await RemoveInvite_Kick(removeNestInvite.userName);
                    break;
                }
                case DenyNestInvite denyNestInvite:
                {
                    await RemoveInvite_GoneOrDenied(denyNestInvite.userName);
                    break;
                }
                case RemoveAllGuests:
                case Stopping:
                {
                    await RemoveAllGuestsNow();
                    break;
                }
            }
        }
        
        private async Task<bool> TryJoin(Join request)
        {
            if (m_us.m_name != request.user.m_name && !m_invitedUsers.Contains(request.user.m_name))
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
        
        private async Task AddInvite(string name)
        {
            if (!m_invitedUsers.Add(name)) return;
                    
            var otherUser = await m_us.m_zone.GetUser(name);
            if (otherUser == null)
            {
                // oops.. remove the invite from our ui
                await RemoveInvite_GoneOrDenied(name);
                return;
            }
                    
            await otherUser.BroadcastXtStr(Modules.NEST_INVITE_TO_NEST, new NestInvite
            {
                m_userName = m_us.m_name
            });
        }
        
        private async Task RemoveInvite_Kick(string name)
        {
            if (!m_invitedUsers.Remove(name)) return;
            await Kick(name);
        }
        
        private async Task Kick(string name)
        {
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
        
        private async Task RemoveInvite_GoneOrDenied(string name)
        {
            if (!m_invitedUsers.Remove(name)) return;
            
            await m_us.BroadcastXtStr(Modules.NEST_REMOVE_GUESTS, new NestInvite
            {
                m_userName = name
            });
        }
        
        private async Task RemoveAllGuestsNow()
        {
            foreach (var user in m_invitedUsers)
            {
                await Kick(user);
            }
            m_invitedUsers.Clear();
        }
    }
    
    public class NestRoom : StatelessRoom
    {
        public PID m_nest;
    }
}