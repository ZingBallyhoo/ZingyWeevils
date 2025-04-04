using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.Str;

namespace BinWeevils.GameServer.Rooms
{
    public abstract class BaseRoom : IWeevilRoomEventHandler
    {
        public Room m_room = null!;

        public abstract ValueTask ClientSentRoomEvent(User user, ClientRoomEvent roomEvent);
    }
}