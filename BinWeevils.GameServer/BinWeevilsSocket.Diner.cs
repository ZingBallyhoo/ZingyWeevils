using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using Microsoft.Extensions.Logging;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleDinerCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.DINER_GRAB_TRAY: // 9#1
                {
                    var tray = new DinerTransferTray();
                    tray.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Diner: Grab Tray - {TrayID}", tray.m_trayId);
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        await dinerRoom.TryGrabTray(tray.m_trayId, user.m_name);
                    });
                    break;
                }
                case Modules.DINER_DROP_TRAY: // 9#2
                {
                    var tray = new DinerTransferTray();
                    tray.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Diner: Drop Tray - {TrayID}", tray.m_trayId);
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        await dinerRoom.TryDropTray(tray.m_trayId, user.m_name);
                    });
                    break;
                }
                case Modules.DINER_CHEF_START: // 9#3
                {
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Diner: Try Start Chef");
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        if (!await dinerRoom.TryStartChef(user.m_name))
                        {
                            m_services.GetLogger().LogWarning("Diner: Start Chef Failed");
                        }
                    });
                    break;
                }
                case Modules.DINER_CHEF_QUIT: // 9#4
                {
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Diner: Try Quit Chef");
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        if (!await dinerRoom.TryQuitChef(user.m_name))
                        {
                            m_services.GetLogger().LogError("Diner: Quit Chef Failed");
                        }
                    });
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"unknown command (diner): \"{message.m_command}\"");
                }
            }
        }
    }
}