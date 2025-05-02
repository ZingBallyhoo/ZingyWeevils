using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class AddPlantRequest
    {
        [PropertyShape(Name = "plantID")] public uint m_plantID { get; set; }
        [PropertyShape(Name = "x")] public short m_x { get; set; }
        [PropertyShape(Name = "z")] public short m_z { get; set; }
    }
    
    [GenerateShape]
    public partial class AddPlantResponse
    {
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        // todo: res, err... but the client doesn't care so
    }
}