using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class ActiveZonesResponse
    {
        [FormUrlEncodedPropertyName("names")] public List<string> m_names { get; set; } = new List<string>();
        [FormUrlEncodedPropertyName("ips")] public List<string> m_ips { get; set; } = new List<string>();
        [FormUrlEncodedPropertyName("oo5")] public List<int> m_outOf5 { get; set; } = new List<int>();
        [FormUrlEncodedPropertyName("responseCode")] public bool m_responseCode { get; set; }
    }
}