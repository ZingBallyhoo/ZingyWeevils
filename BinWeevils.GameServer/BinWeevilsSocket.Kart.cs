using BinWeevils.GameServer.Actors;
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
            var clientMessage = new KartRequestHeader();
            clientMessage.Deserialize(ref reader);

            switch (clientMessage.m_command)
            {
                case Modules.KART_JOIN_GAME:
                {
                    KartMessageHandler<KartJoinGameRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_LEFT_GAME:
                {
                    KartMessageHandler<KartLeaveGameRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_USER_READY:
                {
                    KartMessageHandler<KartUserReadyRequest>(ref reader); // empty
                    break;
                }
                case Modules.KART_DRIVEN_OFF:
                {
                    KartMessageHandler<KartDrivenOffRequest>(ref reader); // empty
                    break;
                }
                
                case Modules.KART_POSITION_UPDATE:
                {
                    KartMessageHandler<KartPositionUpdate>(ref reader);
                    break;
                }
                case Modules.KART_JUMP:
                {
                    KartMessageHandler<KartJump>(ref reader);
                    break;
                }
                case Modules.KART_SPIN_OUT:
                {
                    KartMessageHandler<KartSpinOut>(ref reader);
                    break;
                }
                case Modules.KART_MULCH_BOMB:
                {
                    KartMessageHandler<KartMulchBomb>(ref reader);
                    break;
                }
                case Modules.KART_HOMING_MULCH:
                {
                    KartMessageHandler<KartHomingMulch>(ref reader); 
                    break;
                }
                case Modules.KART_DEPLOY_HOMING_MULCH:
                {
                    KartMessageHandler<KartDeployHomingMulch>(ref reader); 
                    break;
                }
                case Modules.KART_EXPLODE_HOMING_MULCH:
                {
                    KartMessageHandler<KartExplodeHomingMulch>(ref reader); 
                    break;
                }
                case Modules.KART_PLUNGE_HOMING_MULCH:
                {
                    KartMessageHandler<KartPlungeHomingMulch>(ref reader); 
                    break;
                }
                case Modules.KART_DETONATE_MULCH_BOMB:
                {
                    KartMessageHandler<KartDetonateMulchBomb>(ref reader);
                    break;
                }
                case Modules.KART_EXPLODING_MULCH:
                {
                    KartMessageHandler<KartExplodingMulch>(ref reader);
                    break;
                }
                case Modules.KART_DETONATE_EXPLODING_MULCH:
                {
                    KartMessageHandler<KartDetonateExplodingMulch>(ref reader);
                    break;
                }
                
                case Modules.KART_FINISH_LINE:
                {
                    KartMessageHandler<KartFinishLineRequest>(ref reader);
                    break;
                }
                case Modules.KART_PING:
                {
                    KartMessageHandler<KartPing>(ref reader);
                    break;
                }
            }
        }
        
        private void KartMessageHandler<T>(ref StrReader reader) where T : struct, IStrClass
        {
            var packet = new T();
            packet.FullyDeserialize(ref reader);
            
            if (packet is not KartPositionUpdate)
            {
                m_services.GetLogger().LogDebug("Kart - {Data}", packet);
            }
            
            var actorSystem = m_services.GetActorSystem();                    
            var us = GetUser();
            actorSystem.Root.Send(us.GetUserData<WeevilData>().GetUserAddress(), new SocketActor.KartSocketMessage(packet));
        }
    }
}