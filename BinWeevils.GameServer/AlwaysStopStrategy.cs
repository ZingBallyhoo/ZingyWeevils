using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer
{
    public class AlwaysStopStrategy : ISupervisorStrategy
    {
        private static readonly ILogger Logger = Log.CreateLogger<AlwaysStopStrategy>();
        
        public PID m_ownerPID;
        
        public void HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception cause, object? message)
        {
            supervisor.StopChildren(child);
            Logger.LogError("{Action} {Owner} because {Actor} failed with {Reason}", SupervisorDirective.Stop, m_ownerPID, child, cause);
        }
    }
}