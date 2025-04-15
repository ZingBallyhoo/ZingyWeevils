using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Business
{
    public class SaveBusinessStateRequest
    {
        [FormUrlEncodedPropertyName("locID")] public uint m_locID { get; set; }
        [FormUrlEncodedPropertyName("signClr")] public uint m_signColor { get; set; }
        [FormUrlEncodedPropertyName("signTxtClr")] public uint m_signTextColor { get; set; }
        [FormUrlEncodedPropertyName("busOpen")] public byte m_busOpen { get; set; }
    }
}