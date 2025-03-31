using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public record CheckVersionRequest
    {
        [FormUrlEncodedPropertyName("version")] public int m_siteVersion { get; set; }
    }
}