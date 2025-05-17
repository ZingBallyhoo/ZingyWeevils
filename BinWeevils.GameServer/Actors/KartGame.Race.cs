using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str.WeevilKart;
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
            
            m_logger.LogInformation("Kart/{PID}: notifying drive off", context.Self);
            GameServerObservability.s_kartGamesStarted.Add(1, new KeyValuePair<string, object?>("players", m_slots.Length));
            
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
            
            if (!m_notifiedDriveOff)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sent drive-off before notified", context.Self, slot.m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
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
                
                m_logger.LogWarning("Kart/{PID}: kicking player {Player} as they did not drive off", context.Self, slot.m_user);
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
            
            context.Send(context.Parent!, new KartGameSlot.CreateNewGameRequest());
            m_logger.LogInformation("Kart/{PID}: starting race sequence", context.Self);
            m_raceSequenceStarted = true;
            m_raceSequenceStartTime = DateTime.UtcNow;
            
            // start rendering
            NotifyAll(context, new KartNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_PREPARE_GAME_NOTIFICATION
            });
            
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(3)), _ => 
            {
                // start traffic light anim
                NotifyAll(context, new KartNotification 
                {
                    m_commandType = Modules.KART,
                    m_command = Modules.KART_GET_READY_NOTIFICATION
                });
            });
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(6)), _ => 
            {
                m_logger.LogInformation("Kart/{PID}: starting race", context.Self);
                m_raceStarted = true;
                
                // begin moving
                NotifyAll(context, new KartNotification 
                {
                    m_commandType = Modules.KART,
                    m_command = Modules.KART_START_RACE_NOTIFICATION
                });
            });
        }
        
        private void HandleGetStartTime(IContext context, int index)
        {
            if (!m_raceSequenceStarted) return;
            if (m_raceStarted) return;
            
            Notify(context, index, Modules.KART_GET_START_TIME, new KartGetStartTimeResponse
            {
                m_time = (uint)(DateTime.UtcNow - m_raceSequenceStartTime).TotalMilliseconds
            });
        }
        
        private async ValueTask<bool> ValidatePosition(IContext context, int index, double x, double z)
        {
            if (x < 1450 && x > 50 && z < 1450 && z > 50)
            {
                // valid pos
                return true;
            }
            
            // invalid pos
            m_logger.LogError("Kart/{PID}: player {PID} sending position out of valid area", context.Self, m_slots[index].m_user);
            await ForceDisconnectPlayer(context, index);
            return false;
        }
        
        private async ValueTask HandlePositionUpdate(IContext context, int index, KartPositionUpdate update)
        {
            if (index != update.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending position update for someone else", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            if (!await ValidatePosition(context, index, update.m_x, update.m_z))
            {
                return;
            }
            // yes, original server would forward this to yourself...
            NotifyAll(context, Modules.KART_POSITION_UPDATE, update);
        }
        
        private async ValueTask HandleJump(IContext context, int index, KartJump jump)
        {
            if (index != jump.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending jump for someone else", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            if (!await ValidatePosition(context, index, jump.m_x, jump.m_z))
            {
                return;
            }
            // yes, original server would forward this to yourself...
            NotifyAll(context, Modules.KART_JUMP, jump);
        }
        
        private async ValueTask HandleSpinOut(IContext context, int index, KartSpinOut spinOut)
        {
            if (index != spinOut.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending spin out for someone else", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            if (!await ValidatePosition(context, index, spinOut.m_x, spinOut.m_z))
            {
                return;
            }
            NotifyAll(context, Modules.KART_SPIN_OUT, spinOut);
        }
        
        private async ValueTask HandleMulchBomb(IContext context, int index, KartMulchBomb mulchBomb)
        {
            if (index != mulchBomb.m_id.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending invalid mulch bomb", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_MULCH_BOMB, mulchBomb);
        }
        
        private async ValueTask HandleDetonateMulchBomb(IContext context, int index, KartDetonateMulchBomb detonateMulchBomb)
        {
            NotifyAllExcept(context, index, Modules.KART_DETONATE_MULCH_BOMB, detonateMulchBomb);
        }
        
        private async ValueTask HandleHomingMulch(IContext context, int index, KartHomingMulch homingMulch)
        {
            if (index != homingMulch.m_id.m_kartID || index != homingMulch.m_creatorKartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending invalid homing mulch", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_HOMING_MULCH, homingMulch);
        }
        
        private async ValueTask HandleDeployHomingMulch(IContext context, int index, KartDeployHomingMulch deployHomingMulch)
        {
            if (index != deployHomingMulch.m_id.m_kartID || index == deployHomingMulch.m_targetKartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending invalid deploy homing mulch", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_DEPLOY_HOMING_MULCH, deployHomingMulch);
        }
        
        private async ValueTask HandleExplodeHomingMulch(IContext context, int index, KartExplodeHomingMulch explodeHomingMulch)
        {
            // sent by the victim
            // about both ems
            NotifyAllExcept(context, index, Modules.KART_EXPLODE_HOMING_MULCH, explodeHomingMulch);
        }
        
        private async ValueTask HandlePlungeHomingMulch(IContext context, int index, KartPlungeHomingMulch plungeHomingMulch)
        {
            if (index != plungeHomingMulch.m_targetKartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending invalid plunge homing mulch", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_PLUNGE_HOMING_MULCH, plungeHomingMulch);
        }
        
        private async ValueTask HandleExplodingMulch(IContext context, int index, KartExplodingMulch mulchBomb)
        {
            if (index != mulchBomb.m_same.m_id.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending invalid exploding mulch", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_EXPLODING_MULCH, mulchBomb);
        }
        
        private async ValueTask HandleDetonateExplodingMulch(IContext context, int index, KartDetonateExplodingMulch detonateExplodingMulch)
        {
            if (index != detonateExplodingMulch.m_id.m_kartID)
            {
                m_logger.LogError("Kart/{PID}: player {PID} sending detonate exploding mulch for someone else", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            NotifyAllExcept(context, index, Modules.KART_DETONATE_EXPLODING_MULCH, detonateExplodingMulch);
        }
    }
}