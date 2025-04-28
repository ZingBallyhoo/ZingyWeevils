using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class BuySeedRequest
    {
        [FormUrlEncodedPropertyName("id")] public uint m_seedTypeID { get; set; }
        [FormUrlEncodedPropertyName("quantity")] public uint m_quantity { get; set; }
        
        public const uint MAX_QUANTITY = 25;
    }
}