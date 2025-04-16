using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct NestGuestJoined
    {
        [StrField] public string m_name;
        [StrField] public int m_joined; // this message is for joining & leaving...
    }
}