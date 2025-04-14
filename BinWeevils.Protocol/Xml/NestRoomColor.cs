using System.Diagnostics.CodeAnalysis;

namespace BinWeevils.Protocol.Xml
{
    public struct NestRoomColor : ISpanParsable<NestRoomColor>, ISpanFormattable
    {
        public sbyte m_r;
        public sbyte m_g;
        public sbyte m_b;
        
        public static NestRoomColor Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out NestRoomColor result)
        {
            return TryParse(s.AsSpan(), provider, out result);
        }

        public static NestRoomColor Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
            {
                throw new InvalidDataException();
            }
            return result;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out NestRoomColor result)
        {
            Span<Range> splits = [Range.All, Range.All, Range.All];
            var count = s.Split(splits, '|');
            if (count != 3)
            {
                result = default;
                return false;
            }
            
            result = new NestRoomColor
            {
                m_r = sbyte.Parse(s[splits[0]]),
                m_g = sbyte.Parse(s[splits[1]]),
                m_b = sbyte.Parse(s[splits[2]]),
            };
            result.Validate();
            return true;
        }
        
        public void Validate()
        {
            const sbyte min = -120;
            const sbyte max = 60;
            
            if (m_r < min || m_r > max || 
                m_g < min || m_g > max || 
                m_b < min || m_b > max)
            {
                throw new InvalidDataException($"invalid NestRoomColor: \"{ToString()}\"");
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            FormattableString formattable = $"{m_r}|{m_g}|{m_b}";
            return formattable.ToString(formatProvider);
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
            IFormatProvider? provider)
        {
            return destination.TryWrite(provider, $"{m_r}|{m_g}|{m_b}", out charsWritten);
        }

        public override string ToString()
        {
            return $"{m_r}|{m_g}|{m_b}";
        }
    }
}