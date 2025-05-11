namespace BinWeevils.Common
{
    public class WeevilWheelsSettings
    {
        public bool Enabled { get; set; }
        public Dictionary<WeevilWheelsTrophyType, uint> TrophyColors { get; set; }
        public Dictionary<uint, WeevilWheelsTrackSettings> Tracks { get; set; }
    }
    
    public class WeevilWheelsTrackSettings
    {
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