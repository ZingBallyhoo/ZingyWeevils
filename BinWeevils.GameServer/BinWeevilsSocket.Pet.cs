using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.Pet;
using Microsoft.Extensions.Logging;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandlePetCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.PET_MODULE_SEND_PET_COMMAND: // 6#7
                {
                    var command = new ClientPetCommand();
                    command.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        var room = await us.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        // todo: decide what to do with name hash
                        
                        m_services.GetLogger().LogDebug("Pet - SendCommand: {PetName} {Command}", command.m_petName, command.m_commandID);
                        
                        await room.BroadcastXtStr(Modules.PET_MODULE_SEND_PET_COMMAND, new ServerPetCommand
                        {
                            m_userID = us.m_id,
                            m_petName = command.m_petName!,
                            m_commandID = command.m_commandID
                        });
                    });
                    break;
                }
                default:
                {
                    m_services.GetLogger().LogWarning("Unknown pet command: {Command} - {Args}", message.m_command.ToString(), string.Join(" ", reader.ReadToEnd()));
                    break;
                }
            }
        }
    }
}