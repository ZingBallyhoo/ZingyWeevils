using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record TurnBasedGameTurnResponse : TurnBasedGameResponse
    {
        [PropertyShape(Name = "winnerFound")] public bool m_winnerFound;
        [PropertyShape(Name = "staleMate")] public bool m_staleMate;
        [PropertyShape(Name = "nextPlayer")] public string m_nextPlayer;
    }
}