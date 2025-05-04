using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial record ServerTimeResponse
    {
        [PropertyShape(Name = "res")] public bool m_result;
        [PropertyShape(Name = "t")] public long m_time;
        // x=y
    }
}