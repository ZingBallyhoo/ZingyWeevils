using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class RemoveItemFromNestRequest
    {
        [PropertyShape(Name = "userID")] public string m_userName { get; set; }
        [PropertyShape(Name = "nestID")] public uint m_nestID { get; set; }
        [PropertyShape(Name = "itemID")] public uint m_itemID { get; set; }
    }
}