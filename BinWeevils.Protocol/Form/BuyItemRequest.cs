using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BuyItemRequest
    {
        [FormUrlEncodedPropertyName("id")] public uint m_id { get; set; }
        [FormUrlEncodedPropertyName("itemColour")] public string m_itemColor { get; set; }
        [FormUrlEncodedPropertyName("shop")] public string m_shop { get; set; }
    }
}