using ArcticFox.Net.Util;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;

namespace BinWeevils.GameServer.Rooms
{
    public interface IStatefulRoom
    {
        ValueTask<VarBag> GetVars();
    }
    
    public class StatefulRoom<T> : BaseRoom, IStatefulRoom where T : VarBag
    {
        public readonly AsyncLockedAccess<T> m_vars;
        
        public StatefulRoom(T vars)
        {
            m_vars = new AsyncLockedAccess<T>(vars);
        }

        public override async ValueTask ClientSentRoomEvent(User user, ClientRoomEvent roomEvent)
        {
            // todo: actually store the state.. (b?)
            // todo: this is a default implementation, should we just trust what is sent?
                    
            // and we should probably validate what is being sent :)
            // === VARS ===
            // gam:
            //   count: amount of slime banked (0-100)
            //   state: bbb
            //          ^ spoon 1
            //           ^ spoon 2
            //            ^ door closed
            // pool hall:
            //   p1-10: name of player on leaderboard
            // dirt valley:
            //   p1-10: name of player on leaderboard
            //   s1-10: leaderboard scores
            // diners:
            //   s: chef name
            //   p{plate}: food id
            //   t{tray}: user name
            
            await m_room.BroadcastXtStr(Modules.INGAME_ROOM_EVENT, new ServerRoomEvent
            {
                m_eventParams = roomEvent.m_eventParams!
            });
        }

        public async ValueTask<VarBag> GetVars()
        {
            using var token = await m_vars.Get();
            return token.m_value;
        }
    }
}