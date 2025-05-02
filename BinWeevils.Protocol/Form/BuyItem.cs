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
    
    [GenerateShape]
    public partial class BuyItemResponse
    {
        [PropertyShape(Name = "res")] public int m_result { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
    }
}