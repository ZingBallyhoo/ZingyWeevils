using System.Text;
using ArcticFox.Codec.Binary;

namespace ArcticFox.PolyType.Amf
{
    public ref struct AmfEncoder
    {
        public GrowingBitWriter m_writer;
        
        public AmfEncoder()
        {
            m_writer = new GrowingBitWriter();
        }
        
        public void PutMarker(Amf0TypeMarker marker)
        {
            m_writer.WriteByte((byte)marker);
        }
        
        public void PutUInt16(ushort value)
        {
            m_writer.WriteUInt16BigEndian(value);
        }

        public void PutUtf8(string value)
        {
            var encoded = Encoding.UTF8.GetBytes(value);
            PutUInt16(checked((ushort)encoded.Length));
            m_writer.WriteBytes(encoded);
        }

        public void PutInt32(int value)
        {
            m_writer.WriteInt32BigEndian(value);
        }

        public void PutDouble(double value)
        {
            m_writer.WriteDoubleBigEndian(value);
        }
        
        public void Dispose()
        {
            m_writer.Dispose();
        }
    }
}