using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BuyPremisesRequest
    {
        [FormUrlEncodedPropertyName("locTypeID")] public uint m_locTypeID { get; set;}
    }
}