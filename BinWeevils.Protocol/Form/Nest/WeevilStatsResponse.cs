using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class WeevilStatsResponse
    {
        [PropertyShape(Name = "level")] public int m_level { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        [PropertyShape(Name = "xp1")] public int m_xpLowerThreshold { get; set; }
        [PropertyShape(Name = "xp2")] public int m_xpUpperThreshold { get; set; }
        [PropertyShape(Name = "food")] public int m_food { get; set; }
        [PropertyShape(Name = "fitness")] public int m_fitness { get; set; }
        [PropertyShape(Name = "happiness")] public int m_happiness { get; set; }
        
        // todo: removed at some point...
        [PropertyShape(Name = "activated")] public int m_activated { get; set; }
        [PropertyShape(Name = "daysRemaining")] public int m_daysRemaining { get; set; }
        
        [PropertyShape(Name = "cs")] public bool m_chatState { get; set; }
        [PropertyShape(Name = "key")] public int m_chatKey { get; set; }
        [PropertyShape(Name = "st")] public int m_serverTime { get; set; }
        [PropertyShape(Name = "hash")] public string m_hash { get; set; }
    }
}