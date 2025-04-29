using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class BuyGardenItemRequest
    {
        [FormUrlEncodedPropertyName("id")] public uint m_id { get; set; }
        [FormUrlEncodedPropertyName("colour")] public string m_color { get; set; }
    }
}