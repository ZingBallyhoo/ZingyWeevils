using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetSetNestDoor
    {
        [StrField] public ServerPetSetNestDoor m_shared;
        [StrField] public byte m_broadcastSwitch;
    }
    
    public partial record struct ServerPetSetNestDoor
    {
        [StrField] public uint m_petID;
        [StrField] public int m_doorID;
    }
}