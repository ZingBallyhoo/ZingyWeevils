using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class AddItemToNestRequest
    {
        [PropertyShape(Name = "userID")] public string m_userName { get; set; }
        
        [PropertyShape(Name = "nestID")] public uint m_nestID { get; set; }
        [PropertyShape(Name = "locationID")] public uint m_locationID { get; set; }
        [PropertyShape(Name = "itemID")] public uint m_itemID { get; set; }
        [PropertyShape(Name = "itemType")] public string m_itemType { get; set; }
        
        [PropertyShape(Name = "currentframe")] public byte m_posAnimationFrame { get; set; }
        [PropertyShape(Name = "fID")] public uint m_furnitureID { get; set; }
        [PropertyShape(Name = "spot")] public byte m_spot { get; set; }
    }
}