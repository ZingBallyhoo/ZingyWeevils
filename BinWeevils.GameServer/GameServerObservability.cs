using System.Diagnostics;

namespace BinWeevils.GameServer
{
    public static class GameServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.GameServer");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
    }
}