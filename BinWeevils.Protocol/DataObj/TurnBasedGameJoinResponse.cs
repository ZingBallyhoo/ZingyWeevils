using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record TurnBasedGameJoinResponse : TurnBasedGameResponse
    {
        [PropertyShape(Name = "gameStart")] public bool m_gameStart;
        [PropertyShape(Name = "joinData")] public string m_joinData;
        [PropertyShape(Name = "gameData")] public string? m_gameData; // for spectators only
    }
}