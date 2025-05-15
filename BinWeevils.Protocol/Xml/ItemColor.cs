using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using StackXML.Str;

namespace BinWeevils.Protocol.Xml
{
    public partial struct ItemColor : ISpanParsable<ItemColor>, ISpanFormattable
    {
        // cant store as sbyte, we need up to 255
        public short m_r;
        public short m_g;
        public short m_b;
        
        public ItemColor()
        {
            m_r = 0;
            m_g = 0;
            m_b = 0;
        }
        
        public ItemColor(uint value)
        {
            m_b = (byte)(value & 0xFF);
            m_g = (byte)((value >> 8) & 0xFF);
            m_r = (byte)((value >> 16) & 0xFF);
            
            // convert from multiplied color to additive
            m_r -= 255;
            m_g -= 255;
            m_b -= 255;
        }
        
        private ItemColor(DelimitedValue value)
        {
            m_r = value.m_r;
            m_g = value.m_g;
            m_b = value.m_b;
        }
        
        public static ItemColor Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ItemColor result)
        {
            return TryParse(s.AsSpan(), provider, out result);
        }

        public static ItemColor Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
            {
                throw new InvalidDataException();
            }
            return result;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ItemColor result)
        {
            result = new ItemColor();
            
            if (s is "-1")
            {
                // this code path should only be used in the context of the client sending the default color of an
                // uncolorable object
                return true;
            }
            
            if (s.StartsWith("0x"))
            {
                if (!uint.TryParse(s.Slice(2), NumberStyles.HexNumber, null, out var parsedHexEarly))
                {
                    return false;
                }
                
                result = new ItemColor(parsedHexEarly);
                return true;
            }
            if (s.Contains(','))
            {
                var reader = new StrReader(s, ',');
                var delimited = new DelimitedValue();
                delimited.Deserialize(ref reader);
                
                result = new ItemColor(delimited);
                return true;
            }
            
            // todo: this could go wrong of course..
            // but we need to parse decimal from original game nest configs
            if (uint.TryParse(s, null, out var parsedDecimal))
            {
                result = new ItemColor(parsedDecimal);
                return true;
            }
            if (uint.TryParse(s, NumberStyles.HexNumber, null, out var parsedHex))
            {
                result = new ItemColor(parsedHex);
                return true;
            }
            return false;
        }
        
        private partial struct DelimitedValue
        {
            [StrField] public short m_r;
            [StrField] public short m_g;
            [StrField] public short m_b;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"{m_r},{m_g},{m_b}";
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            return destination.TryWrite(provider, $"{m_r},{m_g},{m_b}", out charsWritten);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }
    }
}