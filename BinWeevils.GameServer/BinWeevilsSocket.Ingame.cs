using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.Actions;
using BinWeevils.Protocol.XmlMessages;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleInGameCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.INGAME_MOVE: // 2#1
                {
                    var move = new ClientMove();
                    move.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var weevil = user.GetUserData<WeevilData>();
                        weevil.m_x.SetValue(move.m_x);
                        weevil.m_z.SetValue(move.m_z);
                        weevil.m_r.SetValue((int)move.m_dir);
                        weevil.m_doorID.SetValue(0);
                        weevil.m_poseID.SetValue(0);
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.INGAME_MOVE, new ServerMove
                        {
                            m_uid = checked((int)weevil.m_user.m_id),
                            m_x = weevil.m_x,
                            m_z = weevil.m_z,
                            m_dir = weevil.m_r
                        }, checked((int)room.m_id));
                    });
                    break;
                }
                case Modules.INGAME_EXPRESSION: // 2#2
                {
                    var expression = new ClientExpression();
                    expression.Deserialize(ref reader);
                    
                    if (!Enum.IsDefined(typeof(EWeevilExpression), expression.m_expressionID))
                    {
                        // todo: log
                        Close();
                        return;
                    }
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var weevil = user.GetUserData<WeevilData>();
                        weevil.m_expressionID.SetValue(expression.m_expressionID);
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.INGAME_EXPRESSION, new ServerExpression
                        {
                            m_uid = checked((int)weevil.m_user.m_id),
                            m_expressionID = weevil.m_expressionID
                        }, checked((int)room.m_id));
                    });
                    break;
                }
                case Modules.INGAME_ACTION: // 2#3
                {
                    HandleInGameCommand_Action(ref reader);
                    break;
                }
                case Modules.INGAME_JOIN_ROOM: // 2#4
                {
                    var joinRoomRequest = new JoinRoomRequest();
                    joinRoomRequest.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        await user.RemoveFromRoom(RoomTypeIDs.DEFAULT);
                        
                        var weevil = user.GetUserData<WeevilData>();
                        weevil.m_poseID.SetValue(0);
                        weevil.m_victor.SetValue(0);
                        weevil.m_x.SetValue(joinRoomRequest.m_entryX);
                        weevil.m_y.SetValue(joinRoomRequest.m_entryY);
                        weevil.m_z.SetValue(joinRoomRequest.m_entryZ);
                        weevil.m_r.SetValue(joinRoomRequest.m_entryDir);
                        weevil.m_locID.SetValue(joinRoomRequest.m_locID);
                        weevil.m_doorID.SetValue(joinRoomRequest.m_entryDoorID);
                        
                        var newRoom = await user.m_zone.GetRoom(joinRoomRequest.m_roomName);
                        await user.MoveTo(newRoom);
                        
                        // todo: should we have a callback from inside the join lock?
                        var allWeevils = await newRoom.GetAllUserData<WeevilData>();
                        var roomVars = new VarList();
                        if (newRoom.GetDataAs<IStatefulRoom>() is {} statefulRoom)
                        {
                            var roomVarBag = await statefulRoom.GetVars();
                            roomVars.m_vars = roomVarBag.GetVars();
                        }
                        await this.BroadcastSys(new JoinRoomResponse
                        {
                            m_action = "joinOK",
                            m_room = checked((int)newRoom.m_id),
                            m_vars = roomVars,
                            m_playerList = new RoomPlayerList
                            {
                                m_room = checked((int)newRoom.m_id),
                                m_players = allWeevils.Select(x => new RoomPlayer
                                {
                                    m_name = x.m_user.m_name,
                                    m_uid = checked((int)x.m_user.m_id),
                                    m_vars = new VarList
                                    {
                                        m_vars = x.GetVars()
                                    }
                                }).ToList()
                            }
                        });
                    });
                    break;
                }
                case Modules.INGAME_ROOM_EVENT:
                {
                    var clientEvent = new ClientRoomEvent();
                    clientEvent.Deserialize(ref reader);
                    
                    //Console.Out.WriteLine($"client sent room event: {clientEvent.m_a} {clientEvent.m_b}");
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        var roomData = room.GetDataAs<IWeevilRoomEventHandler>();
                        if (roomData == null) return;
                        await roomData.ClientSentRoomEvent(user, clientEvent);
                    });
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"unknown command (ingame): {message.m_command}");
                    break;
                }
            }
        }

        private void HandleInGameCommand_Action(ref StrReader reader)
        {
            var action = new ClientAction();
            action.Deserialize(ref reader);
            
            if (!Enum.IsDefined(typeof(EWeevilAction), action.m_actionID))
            {
                // todo: log
                Close();
                return;
            }
                    
            if (action.m_endPoseID != -1 && !Enum.IsDefined(typeof(EWeevilAction), action.m_endPoseID))
            {
                // todo: log
                Close();
                return;
            }
                    
            m_taskQueue.Enqueue(async () => { await HandleInGameCommand_Action_Async(action); });
        }

        private async Task HandleInGameCommand_Action_Async(ClientAction action)
        {
            var user = GetUser();
            var room = await user.GetRoom();
            if (room.IsLimbo()) return;
                        
            var weevil = user.GetUserData<WeevilData>();
                        
            HandleInGameCommand_Action_UpdateVars(action, weevil);

            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
            await broadcaster.BroadcastXtStr(Modules.INGAME_ACTION, new ServerAction
            {
                m_userID = checked((int)weevil.m_user.m_id),
                m_actionID = action.m_actionID,
                m_extraParams = action.m_extraParams
            });
        }

        private static void HandleInGameCommand_Action_UpdateVars(ClientAction action, WeevilData weevil)
        {
            Console.Out.WriteLine($"{(EWeevilAction)action.m_actionID} {action.m_extraParams}");

            var extraParamsReader = new StrReader(action.m_extraParams, ',');
            switch ((EWeevilAction)action.m_actionID)
            {
                case EWeevilAction.JUMP_TO:
                {
                    var jumpTo = new JumpToAction();
                    jumpTo.Deserialize(ref extraParamsReader);
                    
                    weevil.m_x.SetValue(jumpTo.m_x);
                    weevil.m_y.SetValue(jumpTo.m_y);
                    weevil.m_z.SetValue(jumpTo.m_z);
                    weevil.m_r.SetValue(jumpTo.m_r);
                    break;
                }
                case EWeevilAction.BOUNCE_TUMBLE:
                {
                    // todo: although the game sends setUvars..
                    // but incorrectly
                    
                    var bounceTumble = new BounceTumbleAction();
                    var lastJump = new BounceTumbleAction_Additional
                    {
                        m_x = bounceTumble.m_x2,
                        m_z = bounceTumble.m_z2
                    };
                    while (extraParamsReader.HasRemaining())
                    {
                        lastJump.Deserialize(ref extraParamsReader);
                    }
                    
                    weevil.m_x.SetValue(lastJump.m_x);
                    weevil.m_y.SetValue(bounceTumble.m_y2); // should be 0
                    weevil.m_z.SetValue(lastJump.m_z);
                    // todo: r. using r2 field isn't correct
                    break;
                }
                case EWeevilAction.TELEPORT:
                {
                    // todo: although the game sends setUvars..
                    var teleport = new TeleportAction();
                    teleport.Deserialize(ref extraParamsReader);
                
                    weevil.m_x.SetValue(teleport.m_x);
                    weevil.m_y.SetValue(teleport.m_y);
                    weevil.m_z.SetValue(teleport.m_z);
                    if (teleport.m_rDest != null)
                    {
                        weevil.m_r.SetValue(teleport.m_rDest.Value);
                    }
                    break;
                }
                
                case EWeevilAction.DROP_IN:
                {
                    // todo: although the game sends setUvars..
                    var dropIn = new DropInAction();
                    dropIn.Deserialize(ref extraParamsReader);
                    
                    weevil.m_x.SetValue(dropIn.m_x1);
                    weevil.m_y.SetValue(0);
                    weevil.m_z.SetValue(dropIn.m_z1);
                    break;
                }
                case EWeevilAction.SUPER_SPEED:
                {
                    var superSpeed = new SuperSpeedAction();
                    superSpeed.Deserialize(ref extraParamsReader);
                    
                    weevil.m_x.SetValue(superSpeed.m_x);
                    weevil.m_z.SetValue(superSpeed.m_z);
                    weevil.m_r.SetValue(superSpeed.m_r);
                    break;
                }
                case EWeevilAction.FLY_UP_TUBE:
                {
                    // todo: although the game sends setUvars..
                    var flyUpTube = new FlyUpTube();
                    flyUpTube.Deserialize(ref extraParamsReader);
                    
                    weevil.m_x.SetValue(flyUpTube.m_xFinal);
                    weevil.m_y.SetValue(flyUpTube.m_yDest);
                    weevil.m_z.SetValue(flyUpTube.m_zFinal);
                    // todo: calc dir (client doesn't send here or in vars...)
                    break;
                }
                // todo: SKATE?
                // todo: which other actions need this
                
                
                // =========================================
                // =========================================
                // VALIDATE ONLY
                // =========================================
                // =========================================
                
                case EWeevilAction.WALK:
                {
                    // todo: invalid to send
                    break;
                }
                case EWeevilAction.JUMP:
                {
                    // (validate only)
                    var jump = new SpinAction();
                    jump.Deserialize(ref extraParamsReader);
                    // todo: validate

                    break;
                }
                // todo: SIT_IN_CAR (validate only)
                // todo: EXIT_CAR (validate only)
                // todo: THROW (validate only)
                case EWeevilAction.SPIN1:
                case EWeevilAction.SPIN2:
                {
                    var spin = new SpinAction();
                    spin.Deserialize(ref extraParamsReader);
                    // todo: validate
                    
                    break;
                }
                case EWeevilAction.JIGGLE:
                {
                    var jiggle = new JiggleAction();
                    jiggle.Deserialize(ref extraParamsReader);
                    // todo: validate
                    
                    break;
                }
                // todo: HOLD_TRAY (validate only)
                // todo: HOLD_ITEM (validate only)
                case EWeevilAction.ADD_ITEM_TO_TRAY:
                {
                    var addItemToTray = new AddItemToTrayAction();
                    addItemToTray.Deserialize(ref extraParamsReader);
                    
                    // todo: validate
                    break;
                }
                case EWeevilAction.TELEPORT_OUT:
                {
                    var teleportOut = new TeleportOutAction();
                    teleportOut.Deserialize(ref extraParamsReader);
                    
                    break;
                }
                // todo: GET_SPUN (validate only)
                case EWeevilAction.FLY_OUT:
                {
                    var flyOut = new FlyOutAction();
                    flyOut.Deserialize(ref extraParamsReader);
                    
                    break;
                }
                // todo: SLIDE_OUT
                case EWeevilAction.DROP_OUT:
                {
                    var dropOut = new DropOutAction();
                    dropOut.Deserialize(ref extraParamsReader);
                    break;
                }
                default:
                {
                    if (action.m_extraParams is "-1")
                    {
                        // no params = send "-1"
                        extraParamsReader.GetString();
                    }
                    break;
                }
            }
            
            if (extraParamsReader.HasRemaining())
            {
                throw new Exception($"didn't fully parse action: {(EWeevilAction)action.m_actionID}");
            }
            
            if (action.m_endPoseID != -1)
            {
                weevil.m_poseID.SetValue(action.m_endPoseID);
            } else
            {
                // don't override the current pose
                // for example, waving while standing tall
            }
        }
    }
}