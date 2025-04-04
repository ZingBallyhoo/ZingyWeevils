using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.XmlMessages;
using StackXML;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleChatCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.CHAT_YOURSELF: // 1#1
                {
                    // client doesn't send this
                    // todo: log
                    Close();
                    break;
                }
                case Modules.CHAT_CHANGE_STATE: // 1#2
                {
                    m_taskQueue.Enqueue(() => this.BroadcastXtStr(Modules.CHAT_CHANGE_STATE, new ServerChatState
                    {
                        m_state = 1
                    }));
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"unknown command (chat): {message.m_command}");
                    Close();
                    break;
                }
            }
        }
        
        private void HandleSfsPubMsg(ReadOnlySpan<char> body)
        {
            var pubMsg = XmlReadBuffer.ReadStatic<PubMsgBody>(body, CDataMode.OnEncoded);
            
            m_taskQueue.Enqueue(async () =>
            {
                var user = GetUser();
                var room = await user.GetRoomOrNull();
                if (room == null) return;
                if (room.IsLimbo()) return;
                
                await room.BroadcastSys(new ServerPubMsgBody
                {
                    m_action = "pubMsg",
                    m_room = checked((int)room.m_id),
                    m_text = pubMsg.m_text,
                    m_user = new UserRecord
                    {
                        m_id = checked((int)user.m_id)
                    }
                });
            });
        }
    }
}