using StackXML.Str;

namespace BinWeevils.GameServer
{
    public class SmartFoxStrMessage
    {
        private const char SEPARATOR = '%';
        
        public static StrReader MakeReader(ReadOnlySpan<char> span)
        {
            if (span[0] != SEPARATOR) throw new InvalidDataException();
            if (span[^1] != SEPARATOR) throw new InvalidDataException();
            
            var trimmedSpan = span.Slice(1, span.Length - 2);
            return new StrReader(trimmedSpan, '%', StandardStrParser.s_instance);
        }
    }
}