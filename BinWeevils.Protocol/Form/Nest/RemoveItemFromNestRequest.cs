using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class RemoveItemFromNestRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userName { get; set; }
        [FormUrlEncodedPropertyName("nestID")] public uint m_nestID { get; set; }
        [FormUrlEncodedPropertyName("itemID")] public uint m_itemID { get; set; }
    }
}