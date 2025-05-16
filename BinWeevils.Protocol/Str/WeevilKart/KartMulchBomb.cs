using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct KartMulchBomb
    {
        [StrField] public KartWeaponID m_id;
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] public double m_speed;
        [StrField] public double m_dir;
        [StrField] public double m_vx;
        [StrField] public double m_vy;
    }
    
    public partial record struct KartDetonateMulchBomb
    {
        [StrField] public KartWeaponID m_id;
    }
}