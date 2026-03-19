using System.Diagnostics.CodeAnalysis;
using PolyType;
using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    [TypeShape(Marshaler = typeof(WordSearchProgressMarshaller))]
    public class WordSearchProgress : ISpanParsable<WordSearchProgress>
    {
        public List<WordSearchSpan> m_spans = [];
        
        public static WordSearchProgress Parse(string s, IFormatProvider? provider)
        {
            return Parse(s.AsSpan(), provider);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out WordSearchProgress result)
        {
            return TryParse(s.AsSpan(), provider, out result);
        }

        public static WordSearchProgress Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
            {
                throw new InvalidDataException($"invalid word search progress: {s}");
            }
            return result;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out WordSearchProgress result)
        {
            if (s.Length == 0)
            {
                // how could this happen? just say no
                result = null;
                return false;
            }
            
            result = new WordSearchProgress();
            if (s is "0")
            {
                // empty
                return true;
            }
            
            var reader = new StrReader(s, '|');
            while (reader.HasRemaining())
            {
                var spanText = reader.GetString();
                var spanReader = new StrReader(spanText, ',');
                
                var span = new WordSearchSpan();
                span.FullyDeserialize(ref spanReader);
                
                span.Normalize();
                span.Validate();
                result.m_spans.Add(span);
            }
            
            return true;
        }

        public override string ToString()
        {
            if (m_spans.Count == 0) return "0";
            return string.Join('|', m_spans.Select(span => span.AsString(',')));
        }
    }

    public class WordSearchProgressMarshaller : IMarshaler<WordSearchProgress, string>
    {
        public string? Marshal(WordSearchProgress? value)
        {
            return value?.ToString();
        }

        public WordSearchProgress? Unmarshal(string? value)
        {
            return WordSearchProgress.Parse(value!, null);
        }
    }
}