using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class PurchaseNestRoomResponse
    {
        [FormUrlEncodedPropertyName("err")] public int m_error { get; set; }
        [FormUrlEncodedPropertyName("id")] public uint m_locID { get; set; }
        
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
        
        public const int ERROR_NONE = 0;
        public const int ERROR_OK = 1;
        public const int ERROR_ALREADY_OWNED = 8;
        public const int ERROR_CANT_AFFORD = 13;
    }
}