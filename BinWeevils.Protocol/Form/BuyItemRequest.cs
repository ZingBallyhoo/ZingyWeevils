using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BuyItemRequest
    {
        [PropertyShape(Name = "id")] public uint m_id { get; set; }
        [PropertyShape(Name = "itemColour")] public string m_itemColor { get; set; }
        [PropertyShape(Name = "shop")] public string m_shop { get; set; }
    }
}