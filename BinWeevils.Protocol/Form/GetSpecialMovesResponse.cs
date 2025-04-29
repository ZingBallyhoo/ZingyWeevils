using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class GetSpecialMovesResponse
    {
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
        [FormUrlEncodedPropertyName("result")] public string m_result { get; set; } // delimited by ";"
    }
}