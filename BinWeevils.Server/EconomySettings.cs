namespace BinWeevils.Server
{
    public class EconomySettings
    {
        public uint MaxMulchPerGame { get; set; } = 5000;
        public uint MaxXpPerGame { get; set; } = 2000;
        public float GameScoreToMulch { get; set; } = 3;
        public float GameScoreToXp { get; set; } = 0.25f;
        //public TimeSpan GameCooldown { get; set; } = TimeSpan.FromSeconds(30);
    }
}