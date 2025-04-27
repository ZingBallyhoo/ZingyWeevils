using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SubmitDailyBrainResponse : BrainInfo
    {
        [FormUrlEncodedPropertyName("mulchEarned")] public uint m_mulchEarned { get; set; }
        [FormUrlEncodedPropertyName("xpEarned")] public uint m_xpEarned { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
    }
}