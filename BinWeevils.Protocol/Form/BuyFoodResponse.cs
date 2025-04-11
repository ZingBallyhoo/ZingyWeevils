using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BuyFoodResponse
    {
        [FormUrlEncodedPropertyName("success")] public int m_success { get; set; }
        [FormUrlEncodedPropertyName("food")] public int m_food { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
    }
}