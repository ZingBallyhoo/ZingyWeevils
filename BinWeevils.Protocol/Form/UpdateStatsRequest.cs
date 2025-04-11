using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class UpdateStatsRequest
    {
        [FormUrlEncodedPropertyName("food")] public byte m_food { get; set; }
        [FormUrlEncodedPropertyName("fitness")] public byte m_fitness { get; set; }
        [FormUrlEncodedPropertyName("happiness")] public byte m_happiness { get; set; }
    }
}