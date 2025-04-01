using ArcticFox.Net;
using ArcticFox.Net.Event;
using BinWeevils.Protocol.XmlMessages;
using StackXML;

namespace BinWeevils.GameServer
{
    public static class BroadcasterExtensions
    {
        public static ValueTask Broadcast(this IBroadcaster bc, Msg msg, CDataMode cdataMode=CDataMode.On)
        {
            using var writeBuffer = new XmlWriteBuffer();
            writeBuffer.PutObject(msg);
            return bc.BroadcastZeroTerminatedAscii(writeBuffer.ToSpan());
        }
    }
}