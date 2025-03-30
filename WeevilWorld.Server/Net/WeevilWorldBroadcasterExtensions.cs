using System;
using System.Buffers.Binary;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using Google.Protobuf;
using WeevilWorld.Protocol;

namespace WeevilWorld.Server.Net
{
    public static class WeevilWorldBroadcasterExtensions
    {
        public static ValueTask Broadcast(this IBroadcaster broadcaster, PacketIDs type, IMessage message)
        {
            var size = message.CalculateSize();

            var buffer = new byte[size + 4];
            var bufferSpan = new Span<byte>(buffer);

            if (size > ushort.MaxValue)
            {
                throw new Exception($"packet too big. ({size} > {ushort.MaxValue})");
            }
            
            BinaryPrimitives.WriteUInt16BigEndian(bufferSpan, (ushort)size);
            BinaryPrimitives.WriteUInt16BigEndian(bufferSpan.Slice(2), (ushort)type);

            var dataSpan = bufferSpan.Slice(4);

            message.WriteTo(dataSpan);

            return broadcaster.BroadcastEventOwningCreation(NetEvent.Create(bufferSpan));
        }
    }
}