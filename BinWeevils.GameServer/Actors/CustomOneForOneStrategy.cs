using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public class CustomOneForOneStrategy : ISupervisorStrategy
    {
        private static readonly ILogger Logger = Log.CreateLogger<CustomOneForOneStrategy>();

        private readonly int _maxNrOfRetries;
        private readonly TimeSpan? _withinTimeSpan;
        
        public CustomOneForOneStrategy(int maxNrOfRetries, TimeSpan? withinTimeSpan)
        {
            _maxNrOfRetries = maxNrOfRetries;
            _withinTimeSpan = withinTimeSpan;
        }
        
        public void HandleFailure(ISupervisor supervisor, PID child, RestartStatistics rs, Exception cause, object? message)
        {
            if (cause is InvalidDataException)
            {
                Logger.LogError(cause, "Escalating failure of {Actor} due to invalid data", child);
                supervisor.EscalateFailure(cause, message);
                return;
            }
            
            if (ShouldStop(rs))
            {
                Logger.LogError(cause, "Escalating failure of {Actor} due too many exceptions", child);
                supervisor.EscalateFailure(cause, message);
                return;
            }
            
            Logger.LogError(cause, "Restarting {Actor} due to exception", child);
            supervisor.RestartChildren(cause, child);
        }
        
        private bool ShouldStop(RestartStatistics rs)
        {
            if (_maxNrOfRetries == 0)
            {
                return true;
            }

            rs.Fail();

            if (rs.NumberOfFailures(_withinTimeSpan) > _maxNrOfRetries)
            {
                rs.Reset();

                return true;
            }

            return false;
        }
    }
}