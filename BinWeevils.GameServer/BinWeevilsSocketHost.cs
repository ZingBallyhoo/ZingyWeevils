using System.Net;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.GameServer.Sfs;
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
                object? roomData = null;
                
                if (location.m_name == "Diner" || location.m_name == "FiggsCafeTerrace" || location.m_name == "TycoonIslandSmoothieBar")
                {
                    int trayCount;
                    int plateCount;

                    switch (location.m_name)
                    {
                        case "Diner":
                        case "FiggsCafeTerrace":
                        {
                            trayCount = 3;
                            plateCount = 12;
                            break;
                        }
                        case "TycoonIslandSmoothieBar":
                        {
                            trayCount = 0;
                            plateCount = 4;
                            break;
                        }
                        default: throw new Exception($"unknown diner loc {location.m_name}");
                    }

                    roomData = new DinerRoom(plateCount, trayCount);
                } else if (location.m_roomEvents)
                {
                    roomData = new StatefulRoom<VarBag>(new VarBag());
                }
                
                roomData ??= new StatelessRoom();
                var room = await zone.CreateRoom(new WeevilRoomDescription(location.m_name)
                {
                    m_maxUsers = 200,
                    m_limbo = false,
                    m_data = roomData
                });
                if (roomData is BaseRoom baseRoom)
                {
                    baseRoom.m_room = room;
                }
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