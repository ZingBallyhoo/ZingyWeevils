using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Business
{
    public class SubmitBusinessNameRequest
    {
        [FormUrlEncodedPropertyName("locID")] public uint m_locID { get; set; }
        [FormUrlEncodedPropertyName("busName")] public string m_name { get; set ;}
    }
}