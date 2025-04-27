using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class GetStoredGardenItemsRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userID { get; set; }
    }
}