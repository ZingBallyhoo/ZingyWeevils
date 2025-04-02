using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct TeleportAction
    {
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] [StrOptional] public int? m_rDest;
    }
}