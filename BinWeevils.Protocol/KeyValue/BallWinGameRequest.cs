using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record BallWinGameRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "userWinner")] public string m_userWinner;
        [PropertyShape(Name = "userLoser")] public string m_userLoser;
    }
}