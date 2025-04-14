using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class GetStoredItemsRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userID { get; set; }
        [FormUrlEncodedPropertyName("mine")] public bool m_mine { get; set; }
    }
}