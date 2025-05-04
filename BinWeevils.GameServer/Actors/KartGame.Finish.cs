using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public partial class KartGame
    {
        private async ValueTask HandleFinishLine(IContext context, int index, uint time)
        {
            ref var slot = ref m_slots[index];
            if (time <= 1000 || slot.m_finishTime != null || !m_raceStarted) 
            {
                // todo: not that useful of a check even. just preventing 0
                m_logger.LogError("Kart/{PID}: player {PID} sending fake finish time", context.Self, m_slots[index].m_user);
                await ForceDisconnectPlayer(context, index);
                return;
            }
            
            slot.m_finishTime = time;
            if (!m_pingPending)
            {
                BeginPing(context);
            } else
            {
                PingAcknowledged(context, index);
            }
        }
        
        private void BeginPing(IContext context)
        {
            var usersToPing = 0;
            foreach (ref var slot in m_slots.AsSpan())
            {
                if (slot.m_user == null) continue;
                if (slot.m_finishTime != null) continue; // no need to ping, they finished
                
                slot.m_pingPending = true;
                Notify(context, slot.m_index, Modules.KART_PING, new Protocol.Str.WeevilKart.Ping());
                
                usersToPing++;
            }
            
            if (usersToPing == 0)
            {
                m_pingPending = false;
                PingsFinished(context);
                return;
            }

            m_pingPending = true;
            context.ReenterAfter(Task.Delay(TimeSpan.FromSeconds(0.5)), _ =>
            {
                // ping timeout
                if (!m_pingPending) return;
                PingsFinished(context);
            });
        }
        
        private void PingAcknowledged(IContext context, int index)
        {
            if (!m_pingPending) return;
            
            ref var slot = ref m_slots[index];
            if (!slot.m_pingPending) return;
            slot.m_pingPending = false;
            
            var allPingsFinished = m_slots.All(x => !x.m_pingPending);
            if (allPingsFinished)
            {
                PingsFinished(context);
            }
        }
        
        private void PingsFinished(IContext context)
        {
            m_pingPending = false;
            
            foreach (var slot in m_slots.Where(x => x.m_ranking == null && x.m_finishTime != null).OrderBy(x => x.m_finishTime))
            {
                m_slots[slot.m_index].m_ranking = ++m_lastAwardedRanking;
            }
            
            var ranks = new KartRanksNotification
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_RANKS
            };
            for (var i = 0; i < m_slots.Length; i++)
            {
                PopulateFinishTime(ranks, i);
                PopulateFinishPos(ranks, i);
            }
            
            NotifyAll(context, ranks);
        }
        
        private void PopulateFinishTime(KartRanksNotification ranks, int index)
        {
            ref var slot = ref m_slots[index];
            var finishTime = slot.m_finishTime ?? 0;
            
            switch (index)
            {
                case 0:
                {
                    ranks.m_player0Time = finishTime;
                    break;
                }
                case 1:
                {
                    ranks.m_player1Time = finishTime;
                    break;
                }
                case 2:
                {
                    ranks.m_player2Time = finishTime;
                    break;
                }
                case 3:
                {
                    ranks.m_player3Time = finishTime;
                    break;
                }
                default:
                {
                    throw new ArgumentException(nameof(index));
                }
            }
        }
            
        private void PopulateFinishPos(KartRanksNotification ranks, int index)
        {
            ref var slot = ref m_slots[index];
            if (slot.m_ranking == null) return;
            
            switch (index)
            {
                case 0:
                {
                    ranks.m_player0Pos = slot.m_ranking;
                    break;
                }
                case 1:
                {
                    ranks.m_player1Pos = slot.m_ranking;
                    break;
                }
                case 2:
                {
                    ranks.m_player2Pos = slot.m_ranking;
                    break;
                }
                case 3:
                {
                    ranks.m_player3Pos = slot.m_ranking;
                    break;
                }
                default:
                {
                    throw new ArgumentException(nameof(index));
                }
            }
        }
    }
}