using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.WeevilKart;
using BinWeevils.Protocol.XmlMessages;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public class SocketActor : IActor
    {
        public required WeevilSocketServices m_services;
        public required User m_user;
        
        private PID m_nest;
        private PID m_buddyList;
        private PID? m_kartGame;
        
        public record KickFromNest(string userName);
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                {
                    await CreateNestNow(context);

                    var buddyListProps = Props.FromProducer(() => new BuddyListActor
                    {
                        m_services = m_services,
                        m_user = m_user
                    }).WithStartDeadline(TimeSpan.FromSeconds(1));
                    // locally this takes about 0.1 seconds to start which protoactor doesn't like...
                    m_buddyList = context.SpawnNamed(buddyListProps, "buddyList");
                    break;
                }
                case KickFromNest kickFromNest:
                {
                    if (kickFromNest.userName == m_user.m_name)
                    {
                        // something has gone horribly wrong
                        context.Stop(context.Self);
                        return;
                    }
                    
                    await m_user.BroadcastXtStr(Modules.NEST_DENY_NEST_INVITE, new NestInvite
                    {
                        m_userName = kickFromNest.userName
                    });
                    await m_user.BroadcastXtStr(Modules.NEST_RETURN_TO_NEST, new NestInvite
                    {
                        m_userName = kickFromNest.userName
                    });
                    break;
                }
                case Stopping:
                {
                    // note: this code triggers for any child!
                    m_user.m_socket?.Close();
                    break;
                }
                
                case BuddyListActor.LoadBuddyListRequest:
                case AddBuddyRequest:
                case BuddyPermissionResponse:
                case SetBuddyVarsRequest:
                case FindBuddyRequest:
                case RemoveBuddyBody:
                {
                    context.Forward(m_buddyList);
                    break;
                }
                
                case JoinGameRequest joinKartGame:
                {
                    if (m_kartGame != null)
                    {
                        throw new InvalidDataException("already in a kart game");
                    }
                    
                    var slotAddress = BinWeevilsSocketHost.GetKartGameAddress(joinKartGame.m_trackName, joinKartGame.m_numPlayers);
                    var slotPID = new PID(context.Self.Address, slotAddress);
                    
                    var request = new KartGameSlot.JoinRequest(context.Self, checked((int)m_user.m_id), joinKartGame.m_kartID);
                    var response = await context.RequestAsync<KartGameSlot.JoinResponse>(slotPID, request);
                    if (response.game != null)
                    {
                        m_kartGame = response.game;
                    }
                    await m_user.BroadcastXtRes(response.clientResponse);
                    break;
                }
                case LeaveGameRequest:
                case UserReadyRequest: 
                case DrivenOffRequest:
                {
                    if (m_kartGame == null) return;
                                        
                    context.Send(m_kartGame, new KartGame.SocketMessage(context.Self, context.Message));
                    if (context.Message is LeaveGameRequest)
                    {
                        m_kartGame = null;
                    }
                    break;
                }
                case KartNotification kartNotification:
                {
                    if (m_kartGame == null) return;
                    if (kartNotification.m_command == Modules.KART_FORCE_DISCONNECT)
                    {
                        m_kartGame = null;
                    }
                    Console.Out.WriteLine(kartNotification);
                    await m_user.BroadcastXtRes(kartNotification);
                    break;
                }
            }
        }

        private async Task CreateNestNow(IContext context)
        {
            var nestProps = Props.FromProducer(() => new NestActor
            {
                m_us = m_user
            });
            m_nest = context.SpawnNamed(nestProps, $"nest");
            
            var nestDesc = new WeevilRoomDescription($"nest_{m_user.m_name}")
            {
                m_creator = m_user,
                m_maxUsers = 20,
                m_isTemporary = true,
                m_data = new NestRoom
                {
                    m_owner = m_user,
                    m_nest = m_nest
                }
            };
            await m_user.m_zone.CreateRoom(nestDesc);
        }
    }
}