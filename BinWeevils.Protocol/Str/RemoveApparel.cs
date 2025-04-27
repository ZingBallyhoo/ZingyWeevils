using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct ClientRemoveApparel
    {
        [StrField] public byte m_apparelSlotID;
    }
    
    public partial record struct ServerRemoveApparel
    {
        [StrField] public ulong m_userID;
        [StrField] public byte m_apparelSlotID;
    }
}