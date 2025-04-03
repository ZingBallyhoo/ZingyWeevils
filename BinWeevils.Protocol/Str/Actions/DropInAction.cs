using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct DropInAction
    {
        [StrField] public double m_x1;
        [StrField] public double m_y;
        [StrField] public double m_z1;
    }
}