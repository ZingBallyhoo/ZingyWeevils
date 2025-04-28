using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class AddPlantRequest
    {
        [FormUrlEncodedPropertyName("plantID")] public uint m_plantID { get; set; }
        [FormUrlEncodedPropertyName("x")] public short m_x { get; set; }
        [FormUrlEncodedPropertyName("z")] public short m_z { get; set; }
    }
}