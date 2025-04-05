using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record BallGameEndedNotification : TurnBasedGameNotification
    {
        [PropertyShape(Name = "userWinner")] public string m_userWinner;
        [PropertyShape(Name = "userLoser")] public string m_userLoser;
        [PropertyShape(Name = "winnerPoints")] public int m_winnerMulch;
        [PropertyShape(Name = "winnerPoints")] public int m_loserMulch;
    }
}