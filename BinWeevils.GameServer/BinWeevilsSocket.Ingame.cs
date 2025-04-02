using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        public void HandleIngameCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.INGAME_MOVE:
                {
                    var move = new ClientMove();
                    move.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var weevil = user.GetUserData<WeevilData>();
                        weevil.m_x.SetValue(move.m_x);
                        weevil.m_z.SetValue(move.m_z);
                        weevil.m_r.SetValue((int)move.m_dir);
                        weevil.m_doorID.SetValue(0);
                        weevil.m_poseID.SetValue(0);
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.INGAME_MOVE, new ServerMove
                        {
                            m_uid = checked((int)weevil.m_user.m_id),
                            m_x = weevil.m_x,
                            m_z = weevil.m_z,
                            m_dir = weevil.m_r
                        }, checked((int)room.m_id));
                    });
                    break;
                }
            }
        }
    }
}