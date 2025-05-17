using BinWeevils.Protocol.Enums;
using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class HasTheUserPlayedRequest
    {
        [PropertyShape(Name = "gameID")] public EGameType m_gameID;
    }
    
    [GenerateShape]
    public partial class HasTheUserPlayedResponse
    {
        [PropertyShape(Name = "hasPlayed")] public int m_hasPlayed;
    }
}