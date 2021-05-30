using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using WeevilWorldProtobuf.Enums;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldSocketHost : SocketHost
    {
        public readonly SmartFoxManager m_smartFoxManager;
        public const string ZONE = "WeevilWorld";

        public WeevilWorldSocketHost()
        {
            m_smartFoxManager = CreateMgr();
        }

        private static SmartFoxManager CreateMgr()
        {
            var services = new ServiceCollection();
            services.UseRegisterAttributeScanner().RegisterFrom(typeof(Zone).Assembly);
            services.AddSingleton<ISystemHandler, WeevilWorldSystemHandler>();

            var provider = services.BuildServiceProvider();
            var mgr = provider.GetRequiredService<SmartFoxManager>();
            return mgr;
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new WeevilWorldSocket(socket, m_smartFoxManager);
        }

        public async ValueTask StartZone()
        {
            var zone = await m_smartFoxManager.CreateZone(ZONE);

            foreach (var type in Enum.GetValues<RoomType>())
            {
                await zone.CreateRoom(new RoomDescription
                {
                    m_name = type.ToString(),
                    m_maxUsers = -1
                });
            }
        }
    }
}