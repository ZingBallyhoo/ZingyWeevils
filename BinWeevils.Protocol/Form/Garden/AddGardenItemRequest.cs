using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class AddGardenItemRequest
    {
        [PropertyShape(Name = "locID")] public uint m_locID { get; set; }
        [PropertyShape(Name = "itemID")] public uint m_itemID { get; set; }
        [PropertyShape(Name = "x")] public short m_x { get; set; }
        [PropertyShape(Name = "z")] public short m_z { get; set; }
    }
}