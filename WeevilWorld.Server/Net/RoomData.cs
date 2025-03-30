using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using WeevilWorld.Protocol;

namespace WeevilWorld.Server.Net
{
    public class RoomData : IRoomEventHandler
    {
        public ValueTask UserJoinedRoom(Room room, User user)
        {
            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
            return broadcaster.Broadcast(PacketIDs.ROOMJOINED_NOTIFICATION, new WeevilWorldProtobuf.Notifications.RoomJoined
            {
                Weevil = user.GetWeevil()
            });
        }

        public ValueTask UserLeftRoom(Room room, User user)
        {
            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
            return broadcaster.Broadcast(PacketIDs.ROOMLEFT_NOTIFICATION, new WeevilWorldProtobuf.Notifications.RoomLeft
            {
                Weevil = user.GetWeevil()
            });
        }
    }
}