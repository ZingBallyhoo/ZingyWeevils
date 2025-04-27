using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BrainInfo
    {
        [FormUrlEncodedPropertyName("levels")] public List<string> m_levels { get; set; }
        [FormUrlEncodedPropertyName("modes")] public byte m_modes { get; set; }
    }
}