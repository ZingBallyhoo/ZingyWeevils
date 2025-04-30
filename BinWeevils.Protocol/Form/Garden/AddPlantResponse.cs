using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class AddPlantResponse
    {
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        // todo: res, err... but the client doesn't care so
    }
}