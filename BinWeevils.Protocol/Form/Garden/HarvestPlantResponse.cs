using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class HarvestPlantResponse
    {
        [PropertyShape(Name = "plantID")] public uint m_plantID { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
    }
}