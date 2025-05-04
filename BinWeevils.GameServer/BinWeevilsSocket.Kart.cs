using System.Text;
using BinWeevils.GameServer.Actors;
using BinWeevils.GameServer.Sfs;
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
                    KartMessageHandler<JoinGameRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_LEFT_GAME:
                {
                    KartMessageHandler<LeaveGameRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_USER_READY:
                {
                    KartMessageHandler<UserReadyRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_DRIVEN_OFF:
                {
                    KartMessageHandler<DrivenOffRequest>(ref reader); // empty
                    break;
                }
                
                case Modules.KART_POSITION_UPDATE:
                {
                    KartMessageHandler<PositionUpdate>(ref reader);
                    break;
                }
                case Modules.KART_FINISH_LINE:
                {
                    KartMessageHandler<FinishLineRequest>(ref reader);
                    break;
                }
                case Modules.KART_PING:
                {
                    KartMessageHandler<Ping>(ref reader);
                    break;
                }
            }
        }
        
        private void KartMessageHandler<T>(ref StrReader reader) where T : IStrClass, new()
        {
            var packet = new T();
            packet.Deserialize(ref reader);
            if (reader.HasRemaining())
            {
                throw new Exception($"didn't fully parse kart message");
            }
            
            if (packet is not PositionUpdate)
            {
                m_services.GetLogger().LogDebug("Kart - {Data}", packet);
            }
            
            var actorSystem = m_services.GetActorSystem();                    
            var us = GetUser();
            actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), new SocketActor.KartSocketMessage(packet));
        }
    }
}