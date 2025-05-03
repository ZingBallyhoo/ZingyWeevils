using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Xml;
using Proto;
using Proto.DependencyInjection;

namespace BinWeevils.GameServer.Actors
{
    public class KartGameSlot : IActor
    {
        private readonly Room m_locRoom;
        private readonly string[] m_kartColors;
        private PID? m_currentGame;
        
        public record JoinRequest(PID user, int userID, byte kartID);
        public record JoinResponse(PID? game, KartResponse clientResponse);
        
        public record CreateNewGameRequest();
        
        public KartGameSlot(Room locRoom, IEnumerable<LocationKart> karts)
        {
            m_locRoom = locRoom;
            m_kartColors = karts.OrderBy(x => x.m_playerID).Select(x => x.m_clr).ToArray();
        }
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                case CreateNewGameRequest:
                {
                    CreateNewGame(context);
                    break;
                }
                case JoinRequest joinRequest:
                {
                    var game = m_currentGame!;
                    context.RequestReenter<KartResponse>(game, joinRequest, task =>
                    {
                        var result = task.Result;
                        
                        context.Respond(new JoinResponse(result.m_success ? game : null, result));
                        return Task.CompletedTask;
                    }, CancellationTokens.FromSeconds(5));
                    break;
                }
                case Terminated:
                {
                    // our pending game failed... make a new one
                    CreateNewGame(context);
                    break;
                }
            }
        }
        
        private void CreateNewGame(IContext context) 
        {
            if (m_currentGame != null) context.Unwatch(m_currentGame);

            var props = context.System.DI().PropsFor<KartGame>(m_locRoom, m_kartColors);
            m_currentGame = context.Spawn(props);
            
            context.Watch(m_currentGame);
        }
    }
}