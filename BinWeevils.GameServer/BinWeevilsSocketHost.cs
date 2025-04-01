using System.Net;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.Xml;
using Microsoft.Extensions.Hosting;

namespace BinWeevils.GameServer
{
    public class BinWeevilsSocketHost : SocketHost, IHostedService
    {
        private readonly SmartFoxManager m_manager;
        private readonly LocationDefinitions m_locationDefinitions;
        private readonly TcpServer m_tcpServer;
        
        public const string ZONE = "Grime";
        
        public BinWeevilsSocketHost(SmartFoxManager manager, LocationDefinitions locationDefinitions)
        {
            m_manager = manager;
            m_locationDefinitions = locationDefinitions;
            m_tcpServer = new TcpServer(this, new IPEndPoint(IPAddress.Loopback, 9339));
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new BinWeevilsSocket(socket, m_manager);
        }
        
        private async Task StartZone(string zoneName)
        {
            var zone = await m_manager.CreateZone(zoneName);
            
            await zone.CreateRoom(new WeevilRoomDescription("Main")
            {
                m_maxUsers = 200,
                m_limbo = true
            });
            await zone.CreateRoom(new WeevilRoomDescription("WeevilWheels")
            {
                m_maxUsers = 200,
                m_limbo = true
            });
            foreach (var location in m_locationDefinitions.m_locations)
            {
                await zone.CreateRoom(new WeevilRoomDescription(location.m_name)
                {
                    m_maxUsers = 200,
                    m_limbo = false
                });
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken=default)
        {
            await StartZone(ZONE);
            
            await base.StartAsync(cancellationToken);
            m_tcpServer.StartAcceptWorker();
        }

        public override async Task StopAsync(CancellationToken cancellationToken=default)
        {
            await base.StopAsync(cancellationToken);
            m_tcpServer.Dispose();
        }
    }
}