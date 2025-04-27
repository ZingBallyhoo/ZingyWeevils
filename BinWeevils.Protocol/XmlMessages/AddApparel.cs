using StackXML.Str;

namespace BinWeevils.Protocol.XmlMessages
{
    public partial record struct ClientAddApparel
    {
        [StrField] public uint m_apparelID;
        [StrField] public string m_rgb;
    }
    
    public partial record struct ServerAddApparel
    {
        [StrField] public ulong m_userID;
        [StrField] public uint m_apparelID;
        [StrField] public string m_rgb;
    }
}