using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitTurnBasedResponse
    {
        [PropertyShape(Name = "mulchEarned")] public int m_mulchEarned { get; set; }
        [PropertyShape(Name = "xpEarned")] public int m_xpEarned { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public int m_xp { get; set; }
    }
}