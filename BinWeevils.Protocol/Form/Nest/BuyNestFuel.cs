using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class BuyNestFuelRequest
    {
        [PropertyShape(Name = "cost")] public byte m_cost;
    }
    
    [GenerateShape]
    public partial class BuyNestFuelResponse
    {
        [PropertyShape(Name = "success")] public bool m_success;
        [PropertyShape(Name = "fuel")] public uint m_fuel;
        [PropertyShape(Name = "mulch")] public int m_mulch;
    }
}