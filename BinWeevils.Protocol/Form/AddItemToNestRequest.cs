using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class AddItemToNestRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userName { get; set; }
        
        [FormUrlEncodedPropertyName("nestID")] public int m_nestID { get; set; }
        [FormUrlEncodedPropertyName("locationID")] public int m_locationID { get; set; }
        [FormUrlEncodedPropertyName("itemID")] public int m_itemID { get; set; }
        [FormUrlEncodedPropertyName("itemType")] public string m_itemType { get; set; }
        
        [FormUrlEncodedPropertyName("currentframe")] public int m_currentPos { get; set; }
        [FormUrlEncodedPropertyName("fID")] public int m_furnitureID { get; set; }
        [FormUrlEncodedPropertyName("spot")] public int m_spot { get; set; }
    }
}