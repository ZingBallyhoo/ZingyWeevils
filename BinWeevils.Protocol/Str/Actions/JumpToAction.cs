using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct JumpToAction
    {
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] public int m_r;
    }
}