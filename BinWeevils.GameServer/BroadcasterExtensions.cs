using ArcticFox.Net;
using ArcticFox.Net.Event;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol.XmlMessages;
using StackXML;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public static class BroadcasterExtensions
    {
        public static ValueTask Broadcast(this IBroadcaster bc, Msg msg, CDataMode cdataMode=CDataMode.On)
        {
            using var writeBuffer = new XmlWriteBuffer
            {
                m_params = new XmlWriteParams
                {
                    m_cdataMode = cdataMode
                }
            };
            writeBuffer.PutObject(msg);
            return bc.BroadcastZeroTerminatedAscii(writeBuffer.ToSpan());
        }
        
        public static ValueTask BroadcastSys(this IBroadcaster bc, MsgBody body, CDataMode cdataMode=CDataMode.On)
        {
            using var writeBuffer = new XmlWriteBuffer();
            writeBuffer.PutObject(BuildSysMessage(body));
            return bc.BroadcastZeroTerminatedAscii(writeBuffer.ToSpan());
        }
        
        public static ValueTask BroadcastXtRes(this IBroadcaster bc, ActionScriptObject obj, int room=-1)
        {
            using var writeBuffer = new XmlWriteBuffer();
            writeBuffer.PutObject(BuildXtResMessage(obj, room));
            return bc.BroadcastZeroTerminatedAscii(writeBuffer.ToSpan());
        }
        
        public static ValueTask BroadcastXtStr<T>(this IBroadcaster bc, string command, T obj, int room=-1) where T : IStrClass
        {
            var writer = SmartFoxStrMessage.MakeWriter();
            try
            {
                writer.PutString("xt");
                writer.PutString(command);
                writer.Put(room);
                obj.Serialize(ref writer);
                return bc.BroadcastZeroTerminatedAscii(writer.AsSpan());
            } finally
            {
                writer.Dispose();
            }
        }
        
        private static Msg BuildXtResMessage(ActionScriptObject obj, int room=-1)
        {
            var body = XmlWriteBuffer.SerializeStatic(obj, CDataMode.Off);

            var resp = new Msg
            {
                m_messageType = "xt",
                m_body = new XtResBody
                {
                    m_action = "xtRes",
                    m_room = room,
                    m_xmlBody = body
                }
            };
            return resp;
        }
        
        private static Msg BuildSysMessage(MsgBody body)
        {
            return new Msg
            {
                m_messageType = "sys",
                m_body = body,
            };
        }
    }
}