using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct NestInvite
    {
        // (for either direction)
        
        [StrField] public string m_userName;
    }
}