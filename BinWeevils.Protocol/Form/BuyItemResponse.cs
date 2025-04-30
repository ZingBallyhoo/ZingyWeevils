using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BuyItemResponse
    {
        [PropertyShape(Name = "res")] public int m_result { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
    }
}