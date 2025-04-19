using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class LoaderAdPathsResponse
    {
        [FormUrlEncodedPropertyName("paths")] public List<string> m_paths { get; set; } = new List<string>();
    }
    
    public class TwoAdPathsResponse
    {
        [FormUrlEncodedPropertyName("ad1Path")] public string m_ad1Path { get; set; } = "0";
        [FormUrlEncodedPropertyName("ad2Path")] public string m_ad2Path { get; set; } = "0";
        [FormUrlEncodedPropertyName("ad1Link")] public string m_ad1Link { get; set; } = "0";
        [FormUrlEncodedPropertyName("ad2Link")] public string m_ad2Link { get; set; } = "0";
    }
}