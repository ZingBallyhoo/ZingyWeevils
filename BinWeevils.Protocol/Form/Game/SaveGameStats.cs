using BinWeevils.Protocol.Enums;
using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class SaveGameStatsRequest 
    {
        [PropertyShape(Name = "gameID")] public EGameType m_gameID;
        [PropertyShape(Name = "awardGiven")] public bool m_awardGiven;
        [PropertyShape(Name = "awardedMulch")] public uint m_awardedMulch; // todo: nullable?
    }
    
    [GenerateShape]
    public partial class SaveGameStatsResponse
    {
        [PropertyShape(Name = "err")] public int m_error;
        
        public const int ERROR_SUCCESS = 1;
    }
}