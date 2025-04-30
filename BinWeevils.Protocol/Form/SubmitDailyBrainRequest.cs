using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitDailyBrainRequest
    {
        [PropertyShape(Name = "levels")] public List<string> m_levels { get; set; }
        [PropertyShape(Name = "score")] public uint m_score { get; set; }
    }
}