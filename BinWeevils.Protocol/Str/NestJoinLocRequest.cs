using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct NestJoinLocRequest
    {
        [StrField] public int m_locID;
        [StrField] public uint m_doorID;
        [StrField] public double m_x;
        [StrField] public double m_y;
        [StrField] public double m_z;
        [StrField] public double m_r;
    }
}