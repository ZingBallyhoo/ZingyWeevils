using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public class AlwaysStopStrategy : ISupervisorStrategy
    {
        private static readonly ILogger Logger = Log.CreateLogger<AlwaysStopStrategy>();
        
        public void HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception cause, object? message)
        {
            supervisor.StopChildren(child);
            Logger.LogError(cause, "Stopping root actor {Actor}", child);
        }
    }
}