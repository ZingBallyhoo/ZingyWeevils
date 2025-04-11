using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class UpdateStatsResponse
    {
        [FormUrlEncodedPropertyName("res")] public int m_result { get; set; }
    }
}