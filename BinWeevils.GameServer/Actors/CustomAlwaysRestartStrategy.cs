using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public class CustomAlwaysRestartStrategy : ISupervisorStrategy
    {
        private static readonly ILogger Logger = Log.CreateLogger<CustomAlwaysRestartStrategy>();
        
        public void HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception cause, object? message)
        {
            Logger.LogError(cause, "Restarting {Actor} due to exception", child);
            supervisor.RestartChildren(cause, child);
        }
    }
}