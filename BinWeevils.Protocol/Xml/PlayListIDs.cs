using System.Diagnostics.CodeAnalysis;
using StackXML.Str;

namespace BinWeevils.Protocol.Xml
{
    public partial struct PlayListIDs : ISpanParsable<PlayListIDs>, ISpanFormattable
    {
        [StrField, StrOptional(0)] public short m_track1 = -1;
        [StrField, StrOptional(1)] public short m_track2 = -1;
        [StrField, StrOptional(2)] public short m_track3 = -1;
        [StrField, StrOptional(3)] public short m_track4 = -1;
        [StrField, StrOptional(4)] public short m_track5 = -1;

        public PlayListIDs()
        {
        }
        
        // todo: remove alloc
        public short[] GetAllValues() => [
            m_track1,
            m_track2,
            m_track3,
            m_track4,
            m_track5
        ];
        
        private bool SanityValidation() 
        {
            var seenUnset = false;
            foreach (var value in GetAllValues())
            {
                if (value < 0)
                {
                    if (value != -1) return false;
                    seenUnset = true;
                } else if (seenUnset)
                {
                    return false;
                }
            }
            return true;
        }

        public static PlayListIDs Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PlayListIDs result)
        {
            return TryParse(s.AsSpan(), provider, out result);
        }

        public static PlayListIDs Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
            {
                throw new InvalidDataException();
            }
            return result;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PlayListIDs result)
        {
            result = new PlayListIDs();

            if (s.Length == 0)
            {
                return true;
            }
            
            var reader = new StrReader(s, ',');
            result.Deserialize(ref reader);

            return result.SanityValidation() && !reader.HasRemaining();
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return this.AsString(',');
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            return this.TryFormat(destination, out charsWritten, ',');
        }
        
        public override string ToString()
        {
            return ToString(null, null);
        }
    }
}