using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct KartExplodingMulch
    {
        [StrField] public KartMulchBomb m_same;
    }
    
    public partial record struct KartDetonateExplodingMulch
    {
        [StrField] public KartWeaponID m_id;
        [StrField] public double m_x;
        [StrField] public double m_z;
    }
}