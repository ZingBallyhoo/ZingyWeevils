using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class BuyGardenItemRequest
    {
        [PropertyShape(Name = "id")] public uint m_id { get; set; }
        [PropertyShape(Name = "colour")] public string m_color { get; set; }
    }
}