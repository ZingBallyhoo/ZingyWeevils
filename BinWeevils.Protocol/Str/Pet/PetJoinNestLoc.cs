using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetJoinNestLoc
    {
        [StrField] public ServerPetJoinNestLoc m_shared;
        [StrField] public byte m_broadcastSwitch;
    }
    
    public partial record struct ServerPetJoinNestLoc
    {
        [StrField] public uint m_petID;
        [StrField] public int m_locID;
        [StrField] public byte m_doorID;
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] public double m_r;
    }
}