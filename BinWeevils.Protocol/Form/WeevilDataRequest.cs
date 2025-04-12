using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class WeevilDataRequest
    {
        [FormUrlEncodedPropertyName("id")] public string m_name { get; set; }
    }
}