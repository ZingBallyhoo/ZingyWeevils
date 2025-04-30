using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BuyFoodRequest
    {
        [PropertyShape(Name = "cost")] public byte m_cost { get; set; }
        [PropertyShape(Name = "energyValue")] public byte m_energyValue { get; set; }
    }
}