using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class BrainInfo
    {
        [PropertyShape(Name = "levels")] public List<string> m_levels { get; set; }
        [PropertyShape(Name = "modes")] public byte m_modes { get; set; }
    }
}