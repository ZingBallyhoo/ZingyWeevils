using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class WeevilDataResponse
    {
        [FormUrlEncodedPropertyName("res")] public int m_result { get; set; }
        [FormUrlEncodedPropertyName("idx")] public uint m_idx { get; set; }
        [FormUrlEncodedPropertyName("weevilDef")] public ulong m_weevilDef { get; set; }
        [FormUrlEncodedPropertyName("level")] public int m_level { get; set; }
        [FormUrlEncodedPropertyName("tycoon")] public int m_tycoon { get; set; }
        [FormUrlEncodedPropertyName("lastLog")] public string m_lastLog { get; set; }
        [FormUrlEncodedPropertyName("dateJoined")] public string m_dateJoined { get; set; }
    }
}