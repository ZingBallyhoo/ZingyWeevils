using BinWeevils.Protocol.Enums;

namespace BinWeevils.Common
{
    public class WeevilWheelsSettings
    {
        public bool Enabled { get; set; }
        public uint MinTimeTrialMulch { get; set; }
        public uint GoldTimeTrialMulch { get; set; }
        public uint MinMultiplayerMulch { get; set; }
        public uint MinMultiplayerXp { get; set; }
        public uint MultiplayerMulchPerBeaten { get; set; }
        public uint MultiplayerXpPerBeaten { get; set; }
        public Dictionary<WeevilWheelsTrophyType, uint> TrophyColors { get; set; }
        public Dictionary<EGameType, WeevilWheelsTrackSettings> Tracks { get; set; }
        
        public Dictionary<byte, EGameType> TrackIDToGame { get; set; } = [];
    }
    
    public class WeevilWheelsTrackSettings
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public string TrophyItem { get; set; }
        public Dictionary<WeevilWheelsTrophyType, TimeSpan> TrophyTimes { get; set; }
    }
    
    public enum WeevilWheelsTrophyType
    {
        None,
        Bronze,
        Silver,
        Gold,
        Count
    }
}