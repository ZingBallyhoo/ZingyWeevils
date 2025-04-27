using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SubmitDailyBrainRequest
    {
        [FormUrlEncodedPropertyName("levels")] public List<string> m_levels { get; set; }
        [FormUrlEncodedPropertyName("score")] public uint m_score { get; set; }
    }
}