using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Enums;
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
                case Modules.PET_MODULE_JOIN_NEST_LOC: // 6#1
                {
                    var joinNestLoc = new ClientPetJoinNestLoc();
                    joinNestLoc.FullyDeserialize(ref reader);
                    
                    var weevilData = GetWeevilData();
                    m_services.GetLogger().LogDebug("Pet({PetID}) - JoinNestLoc: {LocID}", joinNestLoc.m_shared.m_petID, joinNestLoc.m_shared.m_locID);
                    m_services.GetActorSystem().Root.Send(weevilData.GetPetManagerAddress(), joinNestLoc);
                    break;
                }
                case Modules.PET_MODULE_EXPRESSION: // 6#3
                {
                    var expression = new ClientPetExpression();
                    expression.FullyDeserialize(ref reader);
                    
                    var weevilData = GetWeevilData();
                    m_services.GetLogger().LogDebug("Pet({PetID}) - Expression: {Expression}", expression.m_petID, (EPetExpression)expression.m_expressionID);
                    m_services.GetActorSystem().Root.Send(weevilData.GetPetManagerAddress(), expression);
                    break;
                }
                case Modules.PET_MODULE_ACTION: // 6#4
                {
                    var action = new ClientPetAction();
                    action.FullyDeserialize(ref reader);
                    
                    var weevilData = GetWeevilData();
                    m_services.GetLogger().LogDebug("Pet({PetID}) - Action: {Action} {ExtraParams} - {StateStr}", action.m_petID, (EPetAction)action.m_actionID, action.m_extraParams, action.m_stateVars);
                    m_services.GetActorSystem().Root.Send(weevilData.GetPetManagerAddress(), action);
                    break;
                }
                case Modules.PET_MODULE_GOT_BALL: // 6#5
                {
                    var petGotBall = new PetGotBall();
                    petGotBall.FullyDeserialize(ref reader);
                    
                    var user = GetUser();
                    var weevilData = user.GetUserData<WeevilData>()!;
                    if (!weevilData.m_myPetIDs.Contains(petGotBall.m_petID)) 
                    {
                        throw new InvalidDataException("sending gotball for someone else's pet");
                    }
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        m_services.GetLogger().LogDebug("Pet({PetID}) - GotBall", petGotBall.m_petID);
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.PET_MODULE_GOT_BALL, petGotBall);
                    });
                    
                    break;
                }
                case Modules.PET_MODULE_RETURN_TO_NEST: // 6#6
                {
                    var returnToNest = new ClientPetGoHome();
                    returnToNest.FullyDeserialize(ref reader);
                    
                    var user = GetUser();
                    var weevilData = user.GetUserData<WeevilData>();
                    
                    m_services.GetLogger().LogDebug("Pet({PetID}) - ReturnToNest: {State}", returnToNest.m_petID, returnToNest.m_petState);
                    m_services.GetActorSystem().Root.Send(weevilData.GetPetManagerAddress(), returnToNest);
                    break;
                }
                case Modules.PET_MODULE_SEND_PET_COMMAND: // 6#7
                {
                    var command = new ClientPetCommand();
                    command.FullyDeserialize(ref reader);
                    
                    if (!Enum.IsDefined(typeof(EPetSkill), command.m_commandID))
                    {
                        throw new InvalidDataException("invalid pet command id");
                    }
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevilData = user.GetUserData<WeevilData>();

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