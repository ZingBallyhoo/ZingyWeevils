using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
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
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        await dinerRoom.TryStartChef(user.m_name);
                    });
                    break;
                }
                case Modules.DINER_CHEF_QUIT: // 9#4
                {
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        var dinerRoom = room.GetData<DinerRoom>();
                        await dinerRoom.TryQuitChef(user.m_name);
                    });
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"unknown command (diner): {message.m_command}");
                    Close();
                    break;
                }
            }
        }
    }
}