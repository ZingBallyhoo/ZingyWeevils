using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str.WeevilKart;
using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer.Actors
{
    public partial class KartGame : IActor
    {
        private struct KartSlot
        {
            public byte m_index;
            public string m_kartColor;
            
            public PID? m_user;
            public int m_userID;
            public bool m_userReady;
            public bool m_drivenOff;
        }
        
        private readonly ILogger<KartGame> m_logger;
        private readonly Room m_locRoom;
        private KartSlot[] m_slots;
        private readonly Dictionary<PID, int> m_playerToSlot;
        private bool m_gameReady;
        private bool m_notifiedDriveOff;
        private bool m_raceSequenceStarted;
        
        public record SocketMessage(PID pid, object message);
        
        public KartGame(Room locRoom, string[] kartColors, ILogger<KartGame> logger) 
        {
            m_logger = logger;
            m_locRoom = locRoom;
            
            m_slots = new KartSlot[kartColors.Length];
            for (byte i = 0; i < m_slots.Length; i++)
            {
                m_slots[i].m_index = i;
                m_slots[i].m_kartColor = kartColors[i];
                m_slots[i].m_userID = -1;
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
                case SocketMessage socketMessage: 
                {
                    await HandleSocketMessage(context, socketMessage.pid, socketMessage.message);
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
                    ForceDisconnectPlayers(context);
                    break;
                }
            }
        }
        
        private async ValueTask HandleSocketMessage(IContext context, PID user, object message)
        {
            Console.Out.WriteLine($"{user} - {message}");
            if (!m_playerToSlot.TryGetValue(user, out var slot))
            {
                return;
            }
            
            switch (message)
            {
                case UserReadyRequest: 
                {
                    await UserInSlotReady(context, slot);
                    break;
                }
                case DrivenOffRequest: 
                {
                    await UserInSlotDrivenOff(context, slot);
                    break;
                }
                case LeaveGameRequest:
                {
                    await PlayerLeftSlot(context, slot);
                    break;
                }
            }
        }
        
        private async ValueTask ForceDisconnectPlayer(IContext context, int slot)
        {
            if (m_notifiedDriveOff)
            {
                // we cant send force dc as it won't work pas this point...
                context.Stop(m_slots[slot].m_user!);
                return;
            }
            context.Send(m_slots[slot].m_user!, new KartNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_FORCE_DISCONNECT
            });
            
            await PlayerLeftSlot(context, slot);
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
                // prevent driving off
                return true;
            }
            
            // this game has "started" but there's nobody in it.. goodbye
            m_logger.LogInformation("Kart game {PID} closing as no players remain", context.Self);
            context.Stop(context.Self);
            return true;
        }
        
        private void ForceDisconnectPlayers(IContext context)
        {
            NotifyAll(context, new KartNotification 
            {
                m_commandType = Modules.KART,
                m_command = Modules.KART_FORCE_DISCONNECT
            });
        }
        
        private void NotifyAll(IContext context, KartNotification notification) 
        {
            foreach (var slot in m_slots)
            {
                if (slot.m_user == null) continue;
                context.Send(slot.m_user, notification);
            }
        }
    }
}