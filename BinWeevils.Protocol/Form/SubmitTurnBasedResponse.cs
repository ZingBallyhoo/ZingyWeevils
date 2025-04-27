using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SubmitTurnBasedResponse
    {
        [FormUrlEncodedPropertyName("mulchEarned")] public int m_mulchEarned { get; set; }
        [FormUrlEncodedPropertyName("xpEarned")] public int m_xpEarned { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public int m_xp { get; set; }
    }
}