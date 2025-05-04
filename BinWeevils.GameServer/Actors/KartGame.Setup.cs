using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public partial class KartGame
    {
        private void HandleJoinRequest(IContext context, KartGameSlot.JoinRequest joinRequest)
        {
            if (m_gameReady)
            {
                context.Respond(BuildJoinFailedResponse());
                return;
            }
                    
            if (joinRequest.kartID >= m_slots.Length)
            {
                context.Respond(BuildJoinFailedResponse());
                return;
            }
                    
            ref var slot = ref m_slots[joinRequest.kartID];
            if (slot.m_user != null)
            {
                context.Respond(BuildJoinFailedResponse());
                return;
            }
                    
            if (!m_playerToSlot.TryAdd(joinRequest.user, slot.m_index)) 
            {
                // already in this game :((
                return;
            }
            slot.m_user = joinRequest.user;
            slot.m_userID = joinRequest.userID;
            TryMakeGameReady(context);
            
            context.Watch(slot.m_user);
            context.Respond(new KartPlayerJoinResponse 
            {
                m_success = true,
                m_commandType = Modules.KART,
                m_command = Modules.KART_JOIN_GAME,
                m_update = BuildFullUpdate()
            });
                    
            var notification = new KartPlayerJoinedNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_JOINED_NOTIFICATION,
                m_update = BuildNotificationUpdate(slot.m_index)
            };
            foreach (var otherSlot in m_slots)
            {
                if (otherSlot.m_index == slot.m_index) continue;
                if (otherSlot.m_user == null) continue;
                context.Send(otherSlot.m_user, notification);
            }
        }
        
        private void TryMakeGameReady(IContext context)
        {
            var ready = true;
            foreach (var slot in m_slots)
            {
                if (slot.m_user != null) continue;
                
                ready = false;
                break;
            }
            
            m_gameReady = ready;
            if (!m_gameReady) return;
            
            context.Send(context.Parent!, new KartGameSlot.CreateNewGameRequest());
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(3)), async _ => 
            {
                await KickNonReadyUsers(context);
            });
        }
        
        private async ValueTask UserInSlotReady(IContext context, int index) 
        {
            ref var slot = ref m_slots[index];
            
            if (!m_gameReady)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sent user ready before the game is ready", context.Self, slot.m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            if (slot.m_userReady) return;
            slot.m_userReady = true;
            
            await TryNotifyDriveOff(context);
        }
        
        private async Task KickNonReadyUsers(IContext context) 
        {
            if (m_notifiedDriveOff) return;
            
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                if (slot.m_userReady) continue;
                
                m_logger.LogWarning("Kart/{PID}: kicking player {Player} as they did not ready up", context.Self, slot.m_user);
                await ForceDisconnectPlayer(context, slot.m_index);
            }
        }
        
        private void PopulateUserID(KartPendingUpdate update, int index)
        {
            ref var slot = ref m_slots[index];

            switch (index)
            {
                case 0: 
                {
                    update.m_player0ID = slot.m_userID;
                    break;
                }
                case 1: 
                {
                    update.m_player1ID = slot.m_userID;
                    break;
                }
                case 2: 
                {
                    update.m_player2ID = slot.m_userID;
                    break;
                }
                case 3: 
                {
                    update.m_player3ID = slot.m_userID;
                    break;
                }
                default:
                {
                    throw new ArgumentException(nameof(index));
                }
            }
        }
        
        private KartResponse BuildJoinFailedResponse()
        {
            return new KartResponse
            {
                m_success = false,
                m_commandType = Modules.KART,
                m_command = Modules.KART_JOIN_GAME,
            };
        }
        
        private void PopulateKartColor(KartPendingUpdate update, int index)
        {
            ref var slot = ref m_slots[index];
            
            switch (index)
            {
                case 0:
                {
                    update.m_player0KartColor = slot.m_kartColor;
                    break;
                }
                case 1:
                {
                    update.m_player1KartColor = slot.m_kartColor;
                    break;
                }
                case 2:
                {
                    update.m_player2KartColor = slot.m_kartColor;
                    break;
                }
                case 3:
                {
                    update.m_player3KartColor = slot.m_kartColor;
                    break;
                }
                default:
                {
                    throw new ArgumentException(nameof(index));
                }
            }
        }
        
        private KartPendingUpdate BuildNotificationUpdate(int index)
        {
            var update = new KartPendingUpdate
            {
                m_player0ID = null,
                m_player1ID = null,
                m_player2ID = null,
                m_player3ID = null,
                m_gameReady = m_gameReady,
            };
            PopulateUserID(update, index);
            PopulateKartColor(update, index);
            return update;
        }
        
        private KartPendingUpdate BuildFullUpdate()
        {
            var update = new KartPendingUpdate
            {
                m_player0ID = -1,
                m_player1ID = -1,
                m_player2ID = -1,
                m_player3ID = -1,
                m_gameReady = m_gameReady,
            };
            foreach (var kartSlot in m_slots)
            {
                PopulateUserID(update, kartSlot.m_index);
                PopulateKartColor(update, kartSlot.m_index);
            }
            return update;
        }
    }
}