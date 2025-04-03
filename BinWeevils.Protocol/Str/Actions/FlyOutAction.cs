using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct FlyOutAction
    {
        [StrField] public double m_xDest;
        [StrField] public double m_yDest;
        [StrField] public double m_zDest;
    }
}