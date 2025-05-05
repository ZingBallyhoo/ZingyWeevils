using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BinWeevils.Server
{
    public static class ApiServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.Server");
        public static readonly Meter s_meter = new Meter("BinWeevils.Server");
        
        public static readonly Counter<int> s_connectionAttempts = s_meter.CreateCounter<int>("bw_connection_attempts");
        public static readonly UpDownCounter<int> s_activeConnections = s_meter.CreateUpDownCounter<int>("bw_active_connections");
        public static readonly Counter<int> s_haggleItemsSold = s_meter.CreateCounter<int>("bw_haggle_items_sold");
        public static readonly Counter<double> s_haggleTotalPayout = s_meter.CreateCounter<double>("bw_haggle_total_payout");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
    }
}