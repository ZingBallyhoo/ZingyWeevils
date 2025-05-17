using BinWeevils.Protocol.Enums;
using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class StartTurnBasedRequest
    {
        [PropertyShape(Name = "gameId")] public ETurnBasedGameType m_gameID { get; set; }
    }
    
    [GenerateShape]
    public partial class StartTurnBasedResponse
    {
        [PropertyShape(Name = "key")] public string m_key;
    }
}