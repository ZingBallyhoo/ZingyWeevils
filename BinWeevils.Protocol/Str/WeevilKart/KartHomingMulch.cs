using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct KartHomingMulch
    {
        [StrField] public KartWeaponID m_id;
        [StrField] public byte m_creatorKartID;
    }
    
    public partial record struct KartDeployHomingMulch
    {
        [StrField] public KartWeaponID m_id;
        [StrField] public byte m_targetKartID;
    }
    
    public partial record struct KartExplodeHomingMulch
    {
        [StrField] public KartWeaponID m_id;
    }
    
    public partial record struct KartPlungeHomingMulch
    {
        [StrField] public KartWeaponID m_id;
        [StrField] public byte m_targetKartID;
    }
}