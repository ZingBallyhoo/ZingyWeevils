using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class UpdateStatsRequest
    {
        [PropertyShape(Name = "food")] public byte m_food { get; set; }
        [PropertyShape(Name = "fitness")] public byte m_fitness { get; set; }
        [PropertyShape(Name = "happiness")] public byte m_happiness { get; set; }
    }
    
    [GenerateShape]
    public partial class UpdateStatsResponse
    {
        [PropertyShape(Name = "res")] public int m_result { get; set; }
    }
}