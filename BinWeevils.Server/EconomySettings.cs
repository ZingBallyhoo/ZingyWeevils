namespace BinWeevils.Server
{
    public class EconomySettings
    {
        public uint MaxMulchPerGame { get; set; } = 5000;
        public float GameScoreToMulch { get; set; } = 3;
        //public TimeSpan GameCooldown { get; set; } = TimeSpan.FromSeconds(30);
    }
}