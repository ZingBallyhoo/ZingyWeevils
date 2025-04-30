using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitTurnBasedRequest
    {
        [PropertyShape(Name = "key")] public string m_authKey { get; set; }
        [PropertyShape(Name = "result")] public int m_gameResult { get; set; }
    }
}