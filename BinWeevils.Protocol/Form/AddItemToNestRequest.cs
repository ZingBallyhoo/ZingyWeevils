using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class AddItemToNestRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userName { get; set; }
        
        [FormUrlEncodedPropertyName("nestID")] public uint m_nestID { get; set; }
        [FormUrlEncodedPropertyName("locationID")] public uint m_locationID { get; set; }
        [FormUrlEncodedPropertyName("itemID")] public uint m_itemID { get; set; }
        [FormUrlEncodedPropertyName("itemType")] public string m_itemType { get; set; }
        
        [FormUrlEncodedPropertyName("currentframe")] public uint m_currentPos { get; set; }
        [FormUrlEncodedPropertyName("fID")] public uint m_furnitureID { get; set; }
        [FormUrlEncodedPropertyName("spot")] public uint m_spot { get; set; }
    }
}