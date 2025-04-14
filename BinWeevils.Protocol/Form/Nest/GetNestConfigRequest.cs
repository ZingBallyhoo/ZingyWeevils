using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class GetNestConfigRequest
    {
        [FormUrlEncodedPropertyName("id")] public string m_userName { get; set; }
    }
}