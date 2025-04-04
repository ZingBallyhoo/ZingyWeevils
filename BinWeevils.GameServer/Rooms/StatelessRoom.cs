using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.Str;

namespace BinWeevils.GameServer.Rooms
{
    public class StatelessRoom : IWeevilRoomEventHandler
    {
        public ValueTask ClientSentRoomEvent(User user, ClientRoomEvent roomEvent)
        {
            return ValueTask.CompletedTask;
        }
    }
}