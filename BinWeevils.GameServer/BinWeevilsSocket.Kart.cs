using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.WeevilKart;
using Microsoft.Extensions.Logging;
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
                case Modules.KART_JOIN_GAME:
                {
                    var joinGame = new JoinGameRequest();
                    joinGame.Deserialize(ref reader);
                    
                    m_services.GetLogger().LogDebug("Kart - JoinGame: {Data}", joinGame);
                    
                    var actorSystem = m_services.GetActorSystem();                    
                    var us = GetUser();
                    actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), joinGame);
                    break;
                }
                case Modules.KART_LEFT_GAME:
                {
                    var leaveGame = new LeaveGameRequest(); // empty
                    leaveGame.Deserialize(ref reader);
                    
                    m_services.GetLogger().LogDebug("Kart - LeaveGame");
                    
                    var actorSystem = m_services.GetActorSystem();
                    var us = GetUser();
                    actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), leaveGame);
                    break;
                }
                case Modules.KART_USER_READY:
                {
                    var userReady = new UserReadyRequest(); // empty
                    userReady.Deserialize(ref reader);
                    
                    m_services.GetLogger().LogDebug("Kart - UserReady");
                    
                    var actorSystem = m_services.GetActorSystem();
                    var us = GetUser();
                    actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), userReady);
                    break;
                }
                case Modules.KART_DRIVEN_OFF:
                {
                    var drivenOff = new DrivenOffRequest(); // empty
                    drivenOff.Deserialize(ref reader);
                    
                    m_services.GetLogger().LogDebug("Kart - DrivenOff");
                    
                    var actorSystem = m_services.GetActorSystem();
                    var us = GetUser();
                    actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), drivenOff);
                    break;
                }
            }
        }
    }
}