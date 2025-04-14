using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class LevelUpResponse
    {
        [FormUrlEncodedPropertyName("level")] public int m_level { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
        [FormUrlEncodedPropertyName("xp1")] public int m_xpLowerThreshold { get; set; }
        [FormUrlEncodedPropertyName("xp2")] public int m_xpUpperThreshold { get; set; }
        
        [FormUrlEncodedPropertyName("st")] public int m_serverTime { get; set; }
        [FormUrlEncodedPropertyName("hash")] public string m_hash { get; set; }
    }
}