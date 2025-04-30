using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class BuySeedRequest
    {
        [PropertyShape(Name = "id")] public uint m_seedTypeID { get; set; }
        [PropertyShape(Name = "quantity")] public uint m_quantity { get; set; }
        
        public const uint MAX_QUANTITY = 25;
    }
}