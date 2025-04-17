using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using Proto;

namespace BinWeevils.GameServer
{
    public class SocketActor : IActor
    {
        public required User m_user;
        
        public record CreateNest();
        public record KickFromNest(string userName);
        
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
    }
}