using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BuyFoodResponse
    {
        [PropertyShape(Name = "success")] public int m_success { get; set; }
        [PropertyShape(Name = "food")] public int m_food { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
    }
}