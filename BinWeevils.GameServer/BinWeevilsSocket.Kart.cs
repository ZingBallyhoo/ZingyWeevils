using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.WeevilKart;
using Proto;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleKartCommand(in XtClientMessage message, ref StrReader reader)
        {
            var clientMessage = new ClientRequest();
            clientMessage.Deserialize(ref reader);

            switch (clientMessage.m_command)
            {
                case "joinGame":
                {
                    var joinGame = new JoinGameRequest();
                    joinGame.Deserialize(ref reader);
                    
                    /*var gameName = $"kart/{joinGame.m_trackName}_{joinGame.m_numPlayers}";
                    var actorSystem = m_services.GetActorSystem();
                    var gamePID = new PID(actorSystem.Address, gameName);
                    actorSystem.Root.RequestAsync<>(gamePID, null);*/
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        await this.BroadcastXtRes(new KartNotification
                        {
                            m_commandType = Modules.KART,
                            m_command = Modules.KART_FORCE_DISCONNECT
                        });
                    });
                    
                    break;
                }
            }
        }
    }
    
    /*public class KartGameActor : IActor
    {
        public record Join(PID pid, User user, int kartID);
        
        public record ForceDisconnectNotification();
        
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Join join:
                {
                    context.Respond(false);
                   // join.
                    break;
                }
            }
            
            return Task.CompletedTask;
        }
    }*/
}