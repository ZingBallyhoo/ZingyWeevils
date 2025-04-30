using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class RemoveGardenItemRequest
    {
        [PropertyShape(Name = "itemID")] public uint m_itemID { get; set; }
    }
}