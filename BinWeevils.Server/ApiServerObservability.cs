using System.Diagnostics;

namespace BinWeevils.Server
{
    public static class ApiServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.Server");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
    }
}