using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.XmlMessages;

namespace BinWeevils.GameServer
{
    public class WeevilSystemHandler : ISystemHandler
    {
        public ValueTask UserJoinedRoom(Room room, User user)
        {
            if (room.IsLimbo()) return ValueTask.CompletedTask;
            
            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
            
            return broadcaster.BroadcastSys(new UserEnterRoomBody
            {
                m_action = "uER",
                m_room = checked((int)room.m_id),
                m_user = new UserJoinRecord
                {
                    m_uid = checked((int)user.m_id),
                    m_name = user.m_name,
                    m_isModerator = false,
                    m_vars = new VarList
                    {
                        m_vars = user.GetUserData<WeevilData>().GetVars()
                    }
                }
            });
        }

        public ValueTask UserLeftRoom(Room room, User user)
        {
            if (room.IsLimbo()) return ValueTask.CompletedTask;
            
            return room.BroadcastSys(new UserLeaveRoomBody
            {
                m_action = "userGone",
                m_room = checked((int)room.m_id),
                m_user = new UserRecord
                {
                    m_id = checked((int)user.m_id)
                }
            });
        }
    }
}