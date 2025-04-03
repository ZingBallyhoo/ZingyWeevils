using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct SuperSpeedAction
    {
        [StrField] public int m_x;
        [StrField] public int m_z;
        [StrField] public int m_r;
        [StrField] public double m_speed;
    }
}