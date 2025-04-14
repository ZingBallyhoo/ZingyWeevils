using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class GetNestStateRequest
    {
        [FormUrlEncodedPropertyName("id")] public string m_userName { get; set; }
    }
}