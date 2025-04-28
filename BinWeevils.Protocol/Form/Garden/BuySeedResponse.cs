using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class BuySeedResponse
    {
        [FormUrlEncodedPropertyName("err")] public int m_error { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
    }
}