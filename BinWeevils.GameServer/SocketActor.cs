using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using Proto;

namespace BinWeevils.GameServer
{
    public class SocketActor : IActor
    {
        public required User m_user;
        
        public record CreateNest();
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case CreateNest:
                {
                    await CreateNestNow(context);
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
                    m_nest = nestActor
                }
            };
            await m_user.m_zone.CreateRoom(nestDesc);
            
            context.Respond(nestActor);
        }
    }
}