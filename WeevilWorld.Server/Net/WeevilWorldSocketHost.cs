using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldSocketHost : SocketHost
    {
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new WeevilWorldSocket(socket);
        }
    }
}