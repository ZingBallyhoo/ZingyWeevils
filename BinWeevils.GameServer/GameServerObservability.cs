using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BinWeevils.GameServer
{
    public static class GameServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.GameServer");
        public static readonly Meter s_meter = new Meter("BinWeevils.GameServer");
        
        public static readonly Counter<int> s_packetsReceived = s_meter.CreateCounter<int>("bw_packets_received");
        public static readonly Counter<int> s_packetsSent = s_meter.CreateCounter<int>("bw_packets_sent");
        public static readonly Counter<int> s_loginAttempts = s_meter.CreateCounter<int>("bw_login_attempts");
        public static readonly Counter<int> s_usersCreated = s_meter.CreateCounter<int>("bw_users_created");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
    }
}