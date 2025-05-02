namespace BinWeevils.Protocol
{
    public static class Extensions
    {
        public static string ToAs3Date(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        public static string ToAs3Date(this DateTimeOffset dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}