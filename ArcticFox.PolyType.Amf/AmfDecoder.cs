using System.Text;
using ArcticFox.Codec.Binary;

namespace ArcticFox.PolyType.Amf
{
    public ref struct AmfDecoder
    {
        public BitReader m_reader;
        
        public AmfDecoder(BitReader reader)
        {
            m_reader = reader;
        }
        
        public byte ReadByte()
        {
            return m_reader.ReadByte();
        }
        
        public Amf0TypeMarker ReadMarker() => (Amf0TypeMarker)ReadByte();
        
        public bool ReadBool()
        {
            return m_reader.ReadByte() != 0;
        }
        
        public ushort ReadUInt16()
        {
            return m_reader.ReadUInt16BigEndian();
        }
        
        public uint ReadUInt32()
        {
            return m_reader.ReadUInt32BigEndian();
        }

        public double ReadDouble()
        {
            return m_reader.ReadDoubleBigEndian();
        }
        
        public string ReadUtf8()
        {
            var length = ReadUInt16();
            var span = m_reader.ReadBytes(length);
            return Encoding.UTF8.GetString(span);
        }

        public int GetRemainingBytes()
        {
            return m_reader.m_dataLength - m_reader.m_dataOffset;
        }
    }
}