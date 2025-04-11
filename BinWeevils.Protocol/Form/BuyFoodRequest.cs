using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BuyFoodRequest
    {
        [FormUrlEncodedPropertyName("cost")] public byte m_cost { get; set; }
        [FormUrlEncodedPropertyName("energyValue")] public byte m_energyValue { get; set; }
    }
}