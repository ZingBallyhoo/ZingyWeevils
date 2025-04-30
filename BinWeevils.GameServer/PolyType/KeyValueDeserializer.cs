using ArcticFox.PolyType.FormEncoded;
using PolyType;

namespace BinWeevils.GameServer.PolyType
{
    public static class KeyValueDeserializer
    {
        public static T Deserialize<T>(ReadOnlySpan<char> text) where T : IShapeable<T>
        {
            var options = new FormOptions 
            {
                m_keyValueDelimiter = ':',
                m_nextPropertyDelimiter = ',',
                m_nextValueDelimiter = '\0', // not supported
                m_throwOnUnknownFields = true
            };
            return options.Deserialize<T>(text);
        }
    }
}