using System.Diagnostics.CodeAnalysis;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public struct KartWeaponID : ISpanFormattable, ISpanParsable<KartWeaponID>
    {
        public byte m_kartID;
        public ushort m_weaponID;

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            FormattableString formattable = $"{m_kartID}_{m_weaponID}";
            return formattable.ToString(formatProvider);
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
            IFormatProvider? provider)
        {
            return destination.TryWrite(provider, $"{m_kartID}_{m_weaponID}", out charsWritten);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public static KartWeaponID Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out KartWeaponID result)
        {
            return TryParse(s.AsSpan(), provider, out result);
        }

        public static KartWeaponID Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
            {
                throw new InvalidDataException();
            }
            return result;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out KartWeaponID result)
        {
            result = new KartWeaponID();
            
            Span<Range> splits = [Range.All, Range.All];
            var splitCount = s.Split(splits, '_');
            if (splitCount != 2) return false;
            
            return byte.TryParse(s[splits[0]], out result.m_kartID) && 
                   ushort.TryParse(s[splits[1]], out result.m_weaponID);
        }
    }
}