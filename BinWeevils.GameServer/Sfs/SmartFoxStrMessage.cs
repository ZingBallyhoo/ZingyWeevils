using StackXML.Str;

namespace BinWeevils.GameServer.Sfs
{
    public class SmartFoxStrMessage
    {
        private const char SEPARATOR = '%';
        
        public static StrReader MakeReader(ReadOnlySpan<char> span)
        {
            if (span[0] != SEPARATOR) throw new InvalidDataException();
            if (span[^1] != SEPARATOR) throw new InvalidDataException();
            
            var trimmedSpan = span.Slice(1, span.Length - 2);
            return new StrReader(trimmedSpan, SEPARATOR, StandardStrParser.s_instance);
        }
        
        public static StrWriter MakeWriter()
        {
            var writer = new StrWriter(SEPARATOR)
            {
                m_separatorAtEnd = true
            };
            writer.PutRaw(SEPARATOR);
            return writer;
        }
    }
}