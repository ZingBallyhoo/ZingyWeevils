using BinWeevils.Protocol.Enums;

namespace BinWeevils.Common
{
    public class SinglePlayerGamesSettings
    {
        public TimeSpan Cooldown { get; set; }
        public Dictionary<EGameType, SinglePlayerGameSettings> Games { get; set; }
    }
    
    public class SinglePlayerGameSettings
    {
        public SinglePlayerGameOneTimeAward? OneTimeAward { get; set; }
    }
    
    public class SinglePlayerGameOneTimeAward
    {
        public uint? Mulch { get; set; }
    }
}