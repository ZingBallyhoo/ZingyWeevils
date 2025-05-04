using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record PositionUpdateRequest
    {
        [StrField] public int m_kartID;
        [StrField] public int m_time;
        [StrField] public double m_x;
        [StrField] public double m_z;
        [StrField] public int m_ry;
        [StrField] public double m_vx;
        [StrField] public double m_vz;
        [StrField] public double m_idk1;
        [StrField] public double m_idk2;
    }
}