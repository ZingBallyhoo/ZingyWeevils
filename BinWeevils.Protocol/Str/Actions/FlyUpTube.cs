using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct FlyUpTube
    {
        [StrField] public int m_x1;
        [StrField] public int m_z1;
        [StrField] public int m_yDest;
        [StrField] public int m_x2;
        [StrField] public int m_z2;
        [StrField] public int m_xFinal;
        [StrField] public int m_zFinal;
    }
}