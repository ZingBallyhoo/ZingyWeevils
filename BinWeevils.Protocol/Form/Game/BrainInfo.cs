using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class BrainInfo
    {
        [PropertyShape(Name = "levels")] public List<string> m_levels { get; set; }
        [PropertyShape(Name = "modes")] public byte m_modes { get; set; }
    }
}