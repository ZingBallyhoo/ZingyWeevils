using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class AdPathsResponse
    {
        [FormUrlEncodedPropertyName("paths")] public List<string> m_paths { get; set; } = new List<string>();
    }
}