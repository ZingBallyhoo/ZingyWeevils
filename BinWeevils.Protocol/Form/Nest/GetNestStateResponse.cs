using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class GetNestStateResponse
    {
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
        [FormUrlEncodedPropertyName("err")] public string m_error { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_weevilXp { get; set; }
        [FormUrlEncodedPropertyName("score")] public int m_score { get; set; }
        [FormUrlEncodedPropertyName("fuel")] public uint m_fuel { get; set; }
        [FormUrlEncodedPropertyName("lastUpdate")] public string m_lastUpdate { get; set; }
    }
}