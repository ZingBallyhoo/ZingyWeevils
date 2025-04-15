using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class BuyPremisesResponse
    {
        [FormUrlEncodedPropertyName("res")] public string m_result { get; set; }
        [FormUrlEncodedPropertyName("locTypeID")] public uint m_locTypeID { get; set;}
        [FormUrlEncodedPropertyName("locID")] public uint m_locID { get; set;}
        
        public const string RESULT_OWNED = "1";
        public const string RESULT_POOR = "2";
        public const string RESULT_SUCCESS = "3";
    }
}