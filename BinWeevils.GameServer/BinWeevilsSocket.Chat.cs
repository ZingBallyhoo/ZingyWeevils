using System.Text.RegularExpressions;
using ArcticFox.Net;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.Actions;
using BinWeevils.Protocol.XmlMessages;
using Microsoft.Extensions.Logging;
using StackXML;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        [GeneratedRegex(@"^(?! )[a-zA-Z?!\-.&* ]{1,38}(?<! )$")]
        private partial Regex ChatMessageRegex { get; }
        
        private void HandleChatCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.CHAT_YOURSELF: // 1#1
                {
                    throw new InvalidDataException("client sent CHAT_YOURSELF which is not expected");
                }
                case Modules.CHAT_CHANGE_STATE: // 1#2
                {
                    m_taskQueue.Enqueue(() =>
                    {
                        m_services.GetLogger().LogDebug("Chat - Change State");

                        return this.BroadcastXtStr(Modules.CHAT_CHANGE_STATE, new ServerChatState
                        {
                            m_state = 1
                        });
                    });
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"unknown command (chat): {message.m_command}");
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
                
                m_services.GetLogger().LogDebug("Chat - Send: {Message}", pubMsg.m_text);
                
                if (!ChatMessageRegex.IsMatch(pubMsg.m_text))
                {
                    throw new InvalidDataException("chat message contains invalid characters");
                }
                
                await room.BroadcastSys(new ServerPubMsgBody
                {
                    m_action = "pubMsg",
                    m_room = checked((int)room.m_id),
                    m_text = pubMsg.m_text,
                    m_user = new UserRecord
                    {
                        m_id = checked((int)user.m_id)
                    }
                }, CDataMode.OnEncoded);
                
                /*if (pubMsg.m_text == "h")
                {
                    var extraParamsWriter = new StrWriter(',');
                    new TeleportOutAction
                    {
                        m_destLocID = 161 // pool
                    }.Serialize(ref extraParamsWriter);
                    
                    var writer = SmartFoxStrMessage.MakeWriter();
                    writer.PutString("xt");
                    writer.PutString(Modules.INGAME_ACTION);
                    writer.Put(room.m_id);
                    new ServerAction
                    {
                        m_userID = checked((int)user.m_id),
                        m_actionID = (int)EWeevilAction.TELEPORT_OUT,
                        m_extraParams = extraParamsWriter.ToString()
                    }.Serialize(ref writer);
                    await room.BroadcastZeroTerminatedAscii(writer.ToString());
                }*/
            });
        }
    }
}