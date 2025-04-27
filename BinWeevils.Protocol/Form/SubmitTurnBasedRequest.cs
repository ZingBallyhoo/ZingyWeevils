using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SubmitTurnBasedRequest
    {
        [FormUrlEncodedPropertyName("key")] public string m_authKey { get; set; }
        [FormUrlEncodedPropertyName("result")] public int m_gameResult { get; set; }
    }
}