using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetExpression
    {
        [StrField] public ServerPetExpression m_shared;
        [StrField] public byte m_broadcastSwitch;
    }
    
    public partial record struct ServerPetExpression
    {
        [StrField] public uint m_petID;
        [StrField] public byte m_expressionID;
    }
}