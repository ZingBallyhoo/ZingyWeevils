using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public partial class KartGame
    {        
        private async ValueTask TryNotifyDriveOff(IContext context) 
        {
            if (m_notifiedDriveOff) return;
            if (!m_gameReady) return;
            
            var allReady = true;
            foreach (var otherSlot in m_slots)
            {
                allReady &= otherSlot.m_userReady || otherSlot.m_user == null;
            }
            
            if (allReady)
            {
                await NotifyDriveOff(context);
            }
        }
        
        private async ValueTask NotifyDriveOff(IContext context)
        {
            if (m_notifiedDriveOff) return;
            
            m_logger.LogInformation("Kart game {PID} notifying drive off", context.Self);
            
            m_notifiedDriveOff = true;
            await m_locRoom.BroadcastXtRes(new KartDriveOffNotification
            {
                m_commandType = Modules.KART_DRIVE_OFF_NOTIFICATION, // !! type
                m_playerCount = m_slots.Length
            });
            
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(3)), async _ => 
            {
                await KickNonDrivenOffUsers(context);
            });
        }
        
        private async ValueTask UserInSlotDrivenOff(IContext context, int index) 
        {
            ref var slot = ref m_slots[index];
            
            if (slot.m_drivenOff) return;
            slot.m_drivenOff = true;
            
            await TryStartRaceSequence(context);
        }
        
        private async Task KickNonDrivenOffUsers(IContext context) 
        {
            if (m_raceSequenceStarted) return;
            
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                if (slot.m_drivenOff) continue;
                
                m_logger.LogWarning("Kart game {PID} kicking player {Player} as they did not drive off", context.Self, slot.m_user);
                await ForceDisconnectPlayer(context, slot.m_index);
            }
        }
        
        private async ValueTask TryStartRaceSequence(IContext context)
        {
            if (m_raceSequenceStarted) return;
            if (!m_notifiedDriveOff) return;
            
            var allReady = true;
            foreach (var otherSlot in m_slots)
            {
                allReady &= otherSlot.m_drivenOff || otherSlot.m_user == null;
            }
            
            if (allReady)
            {
                await StartRaceSequence(context);
            }
        }
        
        private async Task StartRaceSequence(IContext context) 
        {
            if (m_raceSequenceStarted) return;
            m_raceSequenceStarted = true;
            
            m_logger.LogInformation("Kart game {PID} starting race sequence", context.Self);
            
            NotifyAll(context, new KartNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_PREPARE_GAME_NOTIFICATION
            });
            
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(3)), async _ => 
            {
                NotifyAll(context, new KartNotification 
                {
                    m_commandType = Modules.KART,
                    m_command = "getReady"
                });
            });
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(6)), async _ => 
            {
                m_logger.LogInformation("Kart game {PID} starting race", context.Self);
                NotifyAll(context, new KartNotification 
                {
                    m_commandType = Modules.KART,
                    m_command = "startRace"
                });
            });
        }
    }
}