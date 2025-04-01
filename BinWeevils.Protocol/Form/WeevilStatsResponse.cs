using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class WeevilStatsResponse
    {
        [FormUrlEncodedPropertyName("level")] public int m_level { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public int m_xp { get; set; }
        [FormUrlEncodedPropertyName("xp1")] public int m_xpLowerThreshold { get; set; }
        [FormUrlEncodedPropertyName("xp2")] public int m_xpUpperThreshold { get; set; }
        [FormUrlEncodedPropertyName("food")] public int m_food { get; set; }
        [FormUrlEncodedPropertyName("fitness")] public int m_fitness { get; set; }
        [FormUrlEncodedPropertyName("happiness")] public int m_happiness { get; set; }
        
        // todo: removed at some point...
        [FormUrlEncodedPropertyName("activated")] public int m_activated { get; set; }
        [FormUrlEncodedPropertyName("daysRemaining")] public int m_daysRemaining { get; set; }
        
        [FormUrlEncodedPropertyName("cs")] public bool m_chatState { get; set; }
        [FormUrlEncodedPropertyName("key")] public int m_chatKey { get; set; }
        [FormUrlEncodedPropertyName("st")] public int m_serverTime { get; set; }
        [FormUrlEncodedPropertyName("hash")] public string m_hash { get; set; }
    }
}