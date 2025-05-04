using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct SlideOutAction
    {
        [StrField] public int m_xStart;
        [StrField] public int m_yStart;
        [StrField] public int m_zStart;
        [StrField] public sbyte m_vx;
        [StrField] public sbyte m_vz;
        [StrField] public byte m_pauseInterval;
        [StrField] public double m_slideFactor;
    }
}