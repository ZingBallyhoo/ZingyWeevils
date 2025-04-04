using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.Str;

namespace BinWeevils.GameServer.Rooms
{
    public interface IWeevilRoomEventHandler
    {
        ValueTask ClientSentRoomEvent(User user, ClientRoomEvent roomEvent);
    }
}