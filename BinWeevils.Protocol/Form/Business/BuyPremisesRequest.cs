using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Business
{
    public class BuyPremisesRequest
    {
        [FormUrlEncodedPropertyName("locTypeID")] public uint m_locTypeID { get; set;}
    }
}