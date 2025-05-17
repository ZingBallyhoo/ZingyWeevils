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
    public partial class SaveGameStatsRequest 
    {
        [PropertyShape(Name = "gameID")] public EGameType m_gameID;
        [PropertyShape(Name = "awardGiven")] public bool m_awardGiven;
        [PropertyShape(Name = "awardedMulch")] public uint? m_awardedMulch;
    }
}