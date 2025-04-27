namespace BinWeevils.Server
{
    public class EconomySettings
    {
        public float ShopXpScalar { get; set; } = 10;
        public float ShopDoshToMulch { get; set; } = 500;
        
        public uint MaxMulchPerGame { get; set; } = 5000;
        public uint MaxXpPerGame { get; set; } = 2000;
        public float GameScoreToMulch { get; set; } = 3;
        public float GameScoreToXp { get; set; } = 0.25f;
        //public TimeSpan GameCooldown { get; set; } = TimeSpan.FromSeconds(30);
        
        public uint DailyBrainMaxScore { get; set; } = 500; // not really but using to scale
        public uint DailyBrainMaxMulch { get; set; } = 5000;
        public uint DailyBrainMaxXp { get; set; } = 1000;
    }
}