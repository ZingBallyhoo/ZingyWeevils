using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Objects;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldSocketHost : SocketHost
    {
        public readonly SmartFoxManager m_smartFoxManager;
        public const string ZONE = "WeevilWorld";

        public static readonly int TURN_BASED_GAME_ROOM_TYPE = RoomTypeIDs.NewType("TurnBasedGame");

        public WeevilWorldSocketHost()
        {
            m_smartFoxManager = CreateMgr();
        }

        private static SmartFoxManager CreateMgr()
        {
            var services = new ServiceCollection();
            services.UseRegisterAttributeScanner().RegisterFrom(typeof(Zone).Assembly);
            services.AddSingleton<ISystemHandler, NullSystemHandler>();

            var provider = services.BuildServiceProvider();
            var mgr = provider.GetRequiredService<SmartFoxManager>();
            return mgr;
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new WeevilWorldSocket(socket, m_smartFoxManager);
        }

        public static string GetNestRoomName(Weevil weevil, long slot)
        {
            return $"NestRoom_Slot{slot}_{weevil.Name}";
        }

        public async ValueTask StartZone()
        {
            var zone = await m_smartFoxManager.CreateZone(ZONE);

            foreach (var type in Enum.GetValues<RoomType>())
            {
                await zone.CreateRoom(new RoomDescription
                {
                    m_name = type.ToString(),
                    m_maxUsers = 200,
                    m_data = new RoomData()
                });
            }
        }
    }
}