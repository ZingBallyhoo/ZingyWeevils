using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class RemoveGardenItemRequest
    {
        [FormUrlEncodedPropertyName("itemID")] public uint m_itemID { get; set; }
    }
}