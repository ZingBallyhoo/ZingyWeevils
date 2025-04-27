using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class MoveGardenItemRequest
    {
        [FormUrlEncodedPropertyName("itemID")] public uint m_itemID { get; set; }
        [FormUrlEncodedPropertyName("x")] public short m_x { get; set; }
        [FormUrlEncodedPropertyName("z")] public short m_z { get; set; }
    }
}