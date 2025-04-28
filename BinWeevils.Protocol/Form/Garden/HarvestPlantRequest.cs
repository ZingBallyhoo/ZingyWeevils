using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class HarvestPlantRequest
    {
        [FormUrlEncodedPropertyName("plantID")] public uint m_plantID { get; set; }
    }
}