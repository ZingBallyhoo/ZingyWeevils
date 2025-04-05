using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record TurnBasedGameJoinSpectateResponse : TurnBasedGameResponse
    {
        [PropertyShape(Name = "gameData")] public string m_gameData;
    }
}