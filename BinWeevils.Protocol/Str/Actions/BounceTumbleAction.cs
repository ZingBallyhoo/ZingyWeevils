using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct BounceTumbleAction
    {
        [StrField] public double m_x2;
        [StrField] public double m_y2;
        [StrField] public double m_z2;
        [StrField] public int m_r2;
    }
    
    public partial record struct BounceTumbleAction_Additional
    {
        [StrField] public double m_x;
        [StrField] public double m_z;
    }
}