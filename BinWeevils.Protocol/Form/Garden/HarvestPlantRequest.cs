using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class HarvestPlantRequest
    {
        [PropertyShape(Name = "plantID")] public uint m_plantID { get; set; }
    }
}