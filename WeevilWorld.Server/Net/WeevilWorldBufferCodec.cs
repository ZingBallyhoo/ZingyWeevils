using System;
using System.Buffers.Binary;
using ArcticFox.Codec;
using ArcticFox.Net.Util;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldBufferCodec : SpanCodec<byte, byte>
    {
        private readonly FixedSizeHeader<byte> m_header = new FixedSizeHeader<byte>(2);
        private readonly SizeBufferer<byte> m_body = new SizeBufferer<byte>();

        public override void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            while (input.Length > 0)
            {
                if (m_header.ConsumeAndGet(ref input, out var header))
                {
                    var size = BinaryPrimitives.ReadUInt16BigEndian(header) + 2; // +sizeof(messageType)
                    m_body.SetSize(size);
                }

                if (!m_body.ConsumeAndGet(ref input, out var body)) break;
                CodecOutput(body, ref state);
                m_body.ResetOffset();
                m_header.ResetOffset();
            }
        }
    }
}