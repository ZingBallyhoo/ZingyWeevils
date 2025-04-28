using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Garden
{
    public class AddPlantResponse
    {
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
        // todo: res, err... but the client doesn't care so
    }
}