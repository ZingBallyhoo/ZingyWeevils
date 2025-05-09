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
                        var user = GetUser();
                        var weevilData = user.GetUserDataAs<WeevilData>()!;

                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var expectedHash = m_services.GetPetsSettings().CalculateNameHash(command.m_petName);
                        if (expectedHash != command.m_petNameHash)
                        {
                            throw new InvalidDataException("invalid pet name hash");
                        }
                        if (!weevilData.m_myPetNames.Contains(command.m_petName!))
                        {
                            throw new InvalidDataException("sending pet command for someone else's pet");
                        }
                        
                        m_services.GetLogger().LogDebug("Pet - SendCommand: {PetName} {Command}", command.m_petName, command.m_commandID);
                        
                        await room.BroadcastXtStr(Modules.PET_MODULE_SEND_PET_COMMAND, new ServerPetCommand
                        {
                            m_userID = user.m_id,
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