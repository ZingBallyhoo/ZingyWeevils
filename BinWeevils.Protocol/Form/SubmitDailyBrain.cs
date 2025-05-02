using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitDailyBrainRequest
    {
        [PropertyShape(Name = "levels")] public List<string> m_levels { get; set; }
        [PropertyShape(Name = "score")] public uint m_score { get; set; }
    }
    
    [GenerateShape]
    public partial class SubmitDailyBrainResponse : BrainInfo
    {
        [PropertyShape(Name = "mulchEarned")] public uint m_mulchEarned { get; set; }
        [PropertyShape(Name = "xpEarned")] public uint m_xpEarned { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
    }
}