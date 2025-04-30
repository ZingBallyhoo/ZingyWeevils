using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class LevelUpResponse
    {
        [PropertyShape(Name = "level")] public int m_level { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        [PropertyShape(Name = "xp1")] public int m_xpLowerThreshold { get; set; }
        [PropertyShape(Name = "xp2")] public int m_xpUpperThreshold { get; set; }
        
        [PropertyShape(Name = "st")] public int m_serverTime { get; set; }
        [PropertyShape(Name = "hash")] public string m_hash { get; set; }
    }
}