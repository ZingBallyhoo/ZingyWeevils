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
                case Modules.PET_MODULE_ACTION: // 6#4
                {
                    var action = new ClientPetAction();
                    action.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevilData = user.GetUserDataAs<WeevilData>()!;

                        var room = await user.GetRoomOrNull();
                        if (room == null) return; // the client likes to send during init
                        if (room.IsLimbo()) return;
                        
                        if (!weevilData.m_myPetIDs.Contains(action.m_petID))
                        {
                            throw new InvalidDataException("sending pet action for someone else's pet");
                        }
                        
                        m_services.GetLogger().LogDebug("Pet({PetID}) - Action: {Action} {ExtraParams} - {StateStr}", action.m_petID, (EPetAction)action.m_actionID, action.m_extraParams, action.m_stateVars);
                        
                    });
                    
                    break;
                }
                case Modules.PET_MODULE_SEND_PET_COMMAND: // 6#7
                {
                    var command = new ClientPetCommand();
                    command.Deserialize(ref reader);
                    
                    if (!Enum.IsDefined(typeof(EPetSkill), command.m_commandID))
                    {
                        throw new InvalidDataException("invalid pet command id");
                    }
                    
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
                        
                        m_services.GetLogger().LogDebug("Pet - SendCommand: {PetName} {Command}", command.m_petName, (EPetSkill)command.m_commandID);
                        
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