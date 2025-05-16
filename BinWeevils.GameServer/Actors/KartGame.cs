using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str.WeevilKart;
using Microsoft.Extensions.Logging;
using Proto;
using StackXML.Str;

namespace BinWeevils.GameServer.Actors
{
    public partial class KartGame : IActor
    {
        private struct KartSlot
        {
            public byte m_index;
            public string m_kartColor;
            
            public PID? m_user;
            public int m_userID = -1;
            public bool m_userReady;
            public bool m_drivenOff;
            
            public ushort m_nextWeaponID = 1;
            
            public bool m_pingPending;
            public uint? m_finishTime;
            public int? m_ranking;

            public KartSlot()
            {
            }
        }
        
        private readonly ILogger<KartGame> m_logger;
        private readonly Room m_locRoom;
        private KartSlot[] m_slots;
        private readonly Dictionary<PID, int> m_playerToSlot;
        private bool m_gameReady;
        private bool m_notifiedDriveOff;
        private bool m_raceSequenceStarted;
        private bool m_raceStarted;
        
        private bool m_pingPending;
        private int m_lastAwardedRanking;
        
        public record UserMessage(PID pid, object message);
        
        public KartGame(Room locRoom, string[] kartColors, ILogger<KartGame> logger) 
        {
            m_logger = logger;
            m_locRoom = locRoom;
            
            m_slots = new KartSlot[kartColors.Length];
            for (byte i = 0; i < m_slots.Length; i++)
            {
                m_slots[i] = new KartSlot
                {
                    m_index = i,
                    m_kartColor = kartColors[i]
                };
            }
            m_playerToSlot = new Dictionary<PID, int>(m_slots.Length);
        }
        
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case KartGameSlot.JoinRequest joinRequest:
                {
                    HandleJoinRequest(context, joinRequest);
                    break;
                }
                case UserMessage userMessage: 
                {
                    await HandleUserMessage(context, userMessage.pid, userMessage.message);
                    break;
                }
                case Terminated playerTerminated:
                {
                    if (m_playerToSlot.Remove(playerTerminated.Who, out int fromSlot))
                    {
                        await PlayerLeftSlot(context, fromSlot);
                    }
                    break;
                }
                case Restarting:
                case Stopping:
                {
                    await ForceDisconnectPlayers(context);
                    break;
                }
            }
        }
        
        private async ValueTask HandleUserMessage(IContext context, PID user, object message)
        {
            if (!m_playerToSlot.TryGetValue(user, out var slot))
            {
                return;
            }
            
            switch (message)
            {
                case KartUserReadyRequest: 
                {
                    await UserInSlotReady(context, slot);
                    break;
                }
                case KartDrivenOffRequest: 
                {
                    await UserInSlotDrivenOff(context, slot);
                    break;
                }
                case KartLeaveGameRequest:
                {
                    await PlayerLeftSlot(context, slot);
                    break;
                }
                case KartPositionUpdate update:
                {
                    await HandlePositionUpdate(context, slot, update);
                    break;
                }
                case KartJump jump:
                {
                    await HandleJump(context, slot, jump);
                    break;
                }
                case KartSpinOut spinOut:
                {
                    await HandleSpinOut(context, slot, spinOut);
                    break;
                }
                case KartMulchBomb mulchBomb:
                {
                    await HandleMulchBomb(context, slot, mulchBomb);
                    break;
                }
                case KartDetonateMulchBomb detonateMulchBomb:
                {
                    await HandleDetonateMulchBomb(context, slot, detonateMulchBomb);
                    break;
                }
                
                case KartFinishLineRequest request:
                {
                    await HandleFinishLine(context, slot, request.m_time);
                    break;
                }
                case KartPing:
                {
                    PingAcknowledged(context, slot);
                    break;
                }
            }
        }
        
        private async ValueTask PlayerLeftSlot(IContext context, int index) 
        {
            ref var slot = ref m_slots[index];
            if (slot.m_user == null) return;
            
            m_playerToSlot.Remove(slot.m_user);
            slot.m_user = null;
            slot.m_userID = -1;
            slot.m_userReady = false;
            slot.m_drivenOff = false;
            
            NotifyAll(context, new KartPlayerLeftNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_LEFT_GAME,
                m_kartID = slot.m_index
            });
            
            if (TryRecycle(context))
            {
                return;
            }
            await TryNotifyDriveOff(context);
            await TryStartRaceSequence(context);
        }
        
        private bool TryRecycle(IContext context) 
        {
            var playerCount = 0;
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                playerCount++;
            }
            
            if (playerCount > 0) return false;
            
            if (!m_gameReady)
            {
                // the game didn't actually try to start yet
                // so this instance can just be reused
                return true;
            }
            
            // this game has "started" but there's nobody in it.. goodbye
            m_logger.LogInformation("Kart/{PID}: closing as no players remain", context.Self);
            context.Stop(context.Self);
            return true;
        }
        
        private async ValueTask ForceDisconnectPlayer(IContext context, int index)
        {
            var user = m_slots[index].m_user;
            if (user == null) return;
            
            await PlayerLeftSlot(context, index);
            
            if (m_notifiedDriveOff)
            {
                // we cant send force dc as it won't work at this point...
                context.Stop(user);
                return;
            }
            context.Send(user, new KartNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_FORCE_DISCONNECT
            });
        }
        
        private async ValueTask ForceDisconnectPlayers(IContext context)
        {
            for (var i = 0; i < m_slots.Length; i++)
            {
                await ForceDisconnectPlayer(context, i);
            }
        }
        
        private void NotifyAll(IContext context, KartNotification notification) 
        {
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                context.Send(slot.m_user, notification);
            }
        }
        
        private void NotifyAll<T>(IContext context, string type, T obj) where T : IStrClass
        {
            NotifyAllExcept(context, -1, type, obj);
        }
        
        private void NotifyAllExcept<T>(IContext context, int idx, string type, T obj) where T : IStrClass
        {
            // todo: this is not great but i'd really like for this to use the serialize-once pattern so...
            var serialized = BroadcasterExtensions.SerializeKartEvent(type, obj);
            var sererEvent = new SocketActor.KartServerEvent(serialized);
            
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                if (slot.m_index == idx) continue;
                context.Send(slot.m_user, sererEvent);
            }
        }
        
        private void Notify<T>(IContext context, int index, string type, T obj) where T : IStrClass
        {
            ref var slot = ref m_slots[index];
            if (slot.m_user == null) return;
            
            // todo: this is not great but i'd really like for this to use the serialize-once pattern so...
            var serialized = BroadcasterExtensions.SerializeKartEvent(type, obj);
            var sererEvent = new SocketActor.KartServerEvent(serialized);
            
            context.Send(slot.m_user, sererEvent);
        }
    }
}