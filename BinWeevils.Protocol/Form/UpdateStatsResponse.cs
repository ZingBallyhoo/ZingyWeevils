using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class UpdateStatsResponse
    {
        [PropertyShape(Name = "res")] public int m_result { get; set; }
    }
}