using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct ClientNestSetDoor
    {
        [StrField] public byte m_doorID;
    }
    
    public partial record struct ServerNestSetDoor
    {
        [StrField] public ulong m_userID;
        [StrField] public byte m_doorID;
    }
}