using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public record UpdateNestItemPositionRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userName { get; set; }
        
        [FormUrlEncodedPropertyName("nestID")] public uint m_nestID { get; set; }
        [FormUrlEncodedPropertyName("itemID")] public uint m_itemID { get; set; }
        [FormUrlEncodedPropertyName("itemType")] public string m_itemType { get; set; }

        [FormUrlEncodedPropertyName("crntPos")] public byte m_posAnimationFrame { get; set; }
        [FormUrlEncodedPropertyName("fID")] public uint m_furnitureID { get; set; }
        [FormUrlEncodedPropertyName("spot")] public byte m_spot { get; set; }
    }
}