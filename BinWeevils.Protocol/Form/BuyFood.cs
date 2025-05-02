using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BuyFoodRequest
    {
        [PropertyShape(Name = "cost")] public byte m_cost { get; set; }
        [PropertyShape(Name = "energyValue")] public byte m_energyValue { get; set; }
    }
    
    [GenerateShape]
    public partial class BuyFoodResponse
    {
        [PropertyShape(Name = "success")] public int m_success { get; set; }
        [PropertyShape(Name = "food")] public int m_food { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
    }
}