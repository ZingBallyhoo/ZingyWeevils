using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SetLocColorRequest
    {
        [PropertyShape(Name = "nestID")] public uint m_nestID { get; set; }
        [PropertyShape(Name = "locID")] public uint m_locID { get; set; }
        [PropertyShape(Name = "col")] public string m_col { get; set; }
    }
}