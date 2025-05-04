using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record KartJump
    {
        [StrField] public byte m_kartID;
        [StrField] public uint m_time;
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] public double m_ry;
        [StrField] public double m_dxOverDt;
        [StrField] public double m_dzOverDt;
        [StrField] public double m_launchSpeedIdk;
    }
}