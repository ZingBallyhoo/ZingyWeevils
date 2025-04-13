using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class GetNestConfigRequest
    {
        [FormUrlEncodedPropertyName("id")] public string m_userName { get; set; }
    }
}