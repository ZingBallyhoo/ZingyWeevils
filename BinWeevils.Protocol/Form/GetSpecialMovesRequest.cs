using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class GetSpecialMovesRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userID { get; set; }
    }
}