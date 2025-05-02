namespace BinWeevils.Server.Services
{
    public class UkTimeProvider : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone { get; }

        public UkTimeProvider() 
        {
            LocalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            // (this handles dst)
        }
    }
}