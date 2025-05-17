using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class SubmitTurnBasedRequest
    {
        [PropertyShape(Name = "key")] public string m_authKey { get; set; }
        [PropertyShape(Name = "result")] public byte m_gameResult { get; set; }
    }
    
    [GenerateShape]
    public partial class SubmitTurnBasedResponse
    {
        [PropertyShape(Name = "mulchEarned")] public uint m_mulchEarned { get; set; }
        [PropertyShape(Name = "xpEarned")] public uint m_xpEarned { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
    }
}