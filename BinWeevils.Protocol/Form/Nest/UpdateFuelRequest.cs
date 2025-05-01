using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class UpdateFuelRequest
    {
        [PropertyShape(Name = "fuel")] public uint m_fuel;
    }
}