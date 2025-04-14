using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SetLocColorRequest
    {
        [FormUrlEncodedPropertyName("nestID")] public uint m_nestID { get; set; }
        [FormUrlEncodedPropertyName("locID")] public uint m_locID { get; set; }
        [FormUrlEncodedPropertyName("col")] public string m_col { get; set; }
    }
}