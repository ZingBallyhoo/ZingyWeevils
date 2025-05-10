using BinWeevils.Protocol.Enums;
using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class PurchaseNestRoomRequest
    {
        [PropertyShape(Name = "roomID")] public ENestRoom m_roomType { get; set; }
    }
    
    [GenerateShape]
    public partial class PurchaseNestRoomResponse
    {
        [PropertyShape(Name = "err")] public int m_error { get; set; }
        [PropertyShape(Name = "id")] public uint m_locID { get; set; }
        
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        
        public const int ERROR_NONE = 0;
        public const int ERROR_OK = 1;
        public const int ERROR_ALREADY_OWNED = 8;
        public const int ERROR_CANT_AFFORD = 13;
    }
}