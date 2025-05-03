using System.Net;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Actors;
using BinWeevils.GameServer.Rooms;
using BinWeevils.GameServer.Sfs;
using BinWeevils.GameServer.TurnBased;
using BinWeevils.Protocol.Xml;
using Microsoft.Extensions.Hosting;
using Proto;

namespace BinWeevils.GameServer
{
    public class BinWeevilsSocketHost : SocketHost, IHostedService
    {
        private readonly SmartFoxManager m_manager;
        private readonly ActorSystem m_actorSystem;
        private readonly LocationDefinitions m_locationDefinitions;
        private readonly TcpServer m_tcpServer;
        
        public const string ZONE = "Grime";
        
        public static readonly int TURN_BASED_GAME_ROOM_TYPE = RoomTypeIDs.NewType("TurnBasedGame");
        
        public BinWeevilsSocketHost(SmartFoxManager manager, ActorSystem actorSystem, 
            LocationDefinitions locationDefinitions)
        {
            m_manager = manager;
            m_actorSystem = actorSystem;
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

                foreach (var gameSlot in location.m_gameSlots)
                {
                    await CreateGameRoom(zone, location.m_name, gameSlot.m_slot, gameSlot.m_gamePath);
                }
                foreach (var obj in location.m_objects)
                {
                    if (obj.m_type != "gameSlot") continue;
                    await CreateGameRoom(zone, location.m_name, obj.m_slot, obj.m_gamePath);
                }

                foreach (var kartSlot in location.m_karts.GroupBy(x => GetKartGameAddress(x.m_track, x.m_numPlayers)))
                {
                    var kartSlotProps = Props.FromProducer(() => new KartGameSlot(room, kartSlot))
                        .WithChildSupervisorStrategy(new AlwaysStopStrategy())
                        .WithGuardianSupervisorStrategy(new CustomAlwaysRestartStrategy());
                    m_actorSystem.Root.SpawnNamed(kartSlotProps, kartSlot.Key);
                }
            }
        }
        
        public static string GetKartGameAddress(ReadOnlySpan<char> trackID, int numPlayers)
        {
            return $"kartSlot/{trackID}/{numPlayers}";
        }
        
        public static string GetTurnBasedRoomName(string location, int slot)
        {
            return $"TurnBased_{location}_{slot}";
        }
        
        private static async ValueTask CreateGameRoom(Zone zone, string location, int slot, string gamePath)
        {
            var gameRoom = await zone.CreateRoom(new RoomDescription(GetTurnBasedRoomName(location, slot))
            {
                m_type = TURN_BASED_GAME_ROOM_TYPE
            });
            
            var gameFn = Path.GetFileNameWithoutExtension(gamePath);
            TurnBasedGame? game = gameFn switch
            {
                "mulch4" => new Mulch4Game(gameRoom),
                "squares" => new SquaresGame(gameRoom),
                "reversi" => new ReversiGame(gameRoom),
                "BallGame2Ball" => new BallGame(gameRoom), // todo: do we care about the number of balls? ig to validate
                "BallGame6Ball" => new BallGame(gameRoom),
                "BallGame12Ball" => new BallGame(gameRoom),
                _ => throw new NotImplementedException($"unknown game: {gameFn}")
            };
            gameRoom.SetData(game);
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