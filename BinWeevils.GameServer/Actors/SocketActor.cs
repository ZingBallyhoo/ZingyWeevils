using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.XmlMessages;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public class SocketActor : IActor
    {
        public required WeevilSocketServices m_services;
        public required User m_user;
        
        private PID m_nest;
        private PID m_buddyList;
        
        public record KickFromNest(string userName);
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                {
                    await CreateNestNow(context);

                    m_buddyList = context.SpawnNamed(Props.FromProducer(() => new BuddyListActor
                    {
                        m_services = m_services,
                        m_user = m_user
                    }), "buddyList");
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
                case Stopping:
                {
                    // note: this code triggers for any child!
                    m_user.m_socket?.Close();
                    break;
                }
                
                case BuddyListActor.LoadBuddyListRequest:
                case AddBuddyRequest:
                case BuddyPermissionResponse:
                case SetBuddyVarsRequest:
                case FindBuddyRequest:
                case RemoveBuddyBody:
                {
                    context.Forward(m_buddyList);
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
            m_nest = context.SpawnNamed(nestProps, $"nest");
            
            var nestDesc = new WeevilRoomDescription($"nest_{m_user.m_name}")
            {
                m_creator = m_user,
                m_maxUsers = 20,
                m_isTemporary = true,
                m_data = new NestRoom
                {
                    m_owner = m_user,
                    m_nest = m_nest
                }
            };
            await m_user.m_zone.CreateRoom(nestDesc);
        }
    }
}