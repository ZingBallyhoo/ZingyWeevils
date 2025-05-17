using BinWeevils.Protocol.Enums;

namespace BinWeevils.Common
{
    public class TurnBasedGamesSettings
    {
        public TimeSpan Cooldown { get; set; }
        public uint BaseMulch { get; set; }
        public uint WinMulch { get; set; }
        public uint BaseXp { get; set; }
        public uint WinXp { get; set; }
        public Dictionary<ETurnBasedGameType, SinglePlayerGameSettings> Games { get; set; }
    }
    
    public class TurnBasedGameSettings
    {
    }
}