using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Actors;
using BinWeevils.GameServer.Rooms;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.Actions;
using BinWeevils.Protocol.XmlMessages;
using Microsoft.Extensions.Logging;
using Proto;
using StackXML;
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
                        
                        m_services.GetLogger().LogDebug("Ingame - Move: x:{X} y:{Z} r:{Dir}", move.m_x, move.m_z, move.m_dir);
                        
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
                        throw new InvalidDataException($"invalid expression {expression.m_expressionID}");
                    }
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        m_services.GetLogger().LogDebug("Ingame - Expression: {Expression}", (EWeevilExpression)expression.m_expressionID);
                        
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
                        
                        m_services.GetLogger().LogDebug("Ingame - JoinRoom: {RoomName}", joinRoomRequest.m_roomName);
                        
                        var newRoom = await TryJoinRoom(joinRoomRequest.m_roomName);
                        if (newRoom == null)
                        {
                            m_services.GetLogger().LogError("Failed to join room: {RoomName}", joinRoomRequest.m_roomName);
                            return;
                        }
                        
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
                case Modules.INGAME_ROOM_EVENT: // 2#5
                {
                    var clientEvent = new ClientRoomEvent();
                    clientEvent.Deserialize(ref reader);
                    
                    //Console.Out.WriteLine($"client sent room event: {clientEvent.m_a} {clientEvent.m_b}");
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Ingame - ClientRoomEvent: Para:{Params} State:{State}", clientEvent.m_eventParams, clientEvent.m_roomState);
                        
                        var roomData = room.GetDataAs<IWeevilRoomEventHandler>();
                        if (roomData == null) return;
                        await roomData.ClientSentRoomEvent(user, clientEvent);
                    });
                    break;
                }
                case Modules.INGAME_GET_ZONE_TIME: // 2#6
                {
                    m_taskQueue.Enqueue(() => 
                    {
                        var ukTime = m_services.GetTimeProvider().GetLocalNow();
                        return this.BroadcastXtStr(Modules.INGAME_GET_ZONE_TIME, new GetZoneTimeResponse
                        {
                            m_dateTime = ukTime.ToAs3Date()
                        });
                    });
                    
                    break;
                }
                case Modules.INGAME_ADD_APPAREL: // 2#7
                {
                    var addApparel = new ClientAddApparel();
                    addApparel.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () => 
                    {
                        var user = GetUser();
                        var weevilData = user.GetUserDataAs<WeevilData>()!;
                        
                        m_services.GetLogger().LogDebug("Ingame - AddApparel: {ID} - {Color}", addApparel.m_apparelID, addApparel.m_rgb);
                        
                        var changed = await m_services.SetApparel(weevilData.m_idx, addApparel.m_apparelID, addApparel.m_rgb);
                        if (!changed)
                        {
                            return;
                        }
                        
                        weevilData.m_apparel.SetValue($"|{addApparel.m_apparelID}:{addApparel.m_rgb}");
                        
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.INGAME_ADD_APPAREL, new ServerAddApparel
                        {
                            m_userID = user.m_id,
                            m_apparelID = addApparel.m_apparelID,
                            m_rgb = addApparel.m_rgb
                        });
                    });
                    
                    break;
                }
                case Modules.INGAME_REMOVE_APPAREL: // 2#8
                {
                    var removeApparel = new ClientRemoveApparel();
                    removeApparel.Deserialize(ref reader);
                    
                    if (removeApparel.m_apparelSlotID != 1)
                    {
                        throw new InvalidDataException($"invalid apparel slot id: {removeApparel.m_apparelSlotID}");
                    }
                    
                    m_taskQueue.Enqueue(async () => 
                    {
                        var user = GetUser();
                        var weevilData = user.GetUserDataAs<WeevilData>()!;
                        
                        m_services.GetLogger().LogDebug("Ingame - RemoveApparel: {Slot}", removeApparel.m_apparelSlotID);
                        
                        var changed = await m_services.RemoveApparel(weevilData.m_idx);
                        if (!changed)
                        {
                            return;
                        }
                        
                        weevilData.m_apparel.SetValue(string.Empty);
                        
                        var room = await user.GetRoom();
                        if (room.IsLimbo()) return;
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.BroadcastXtStr(Modules.INGAME_REMOVE_APPAREL, new ServerRemoveApparel
                        {
                            m_userID = user.m_id,
                            m_apparelSlotID = removeApparel.m_apparelSlotID
                        });
                    });
                    
                    break;
                }
                case Modules.INGAME_CHANGE_WEEVIL_DEF: // 2#10
                {
                    var changeWeevilDef = new ChangeWeevilDef();
                    changeWeevilDef.Deserialize(ref reader);
                    
                    var normalizedDef = new WeevilDef(changeWeevilDef.m_weevilDef);
                    var normalizedDefNum = normalizedDef.AsNumber();
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevilData = user.GetUserDataAs<WeevilData>();
                        
                        var dbDef = await m_services.GetWeevilDef(weevilData!.m_idx.GetValue());
                        if (dbDef != normalizedDefNum)
                        {
                            throw new InvalidDataException($"sent different def in ChangeWeevilDef than what is stored in db. db:{dbDef}, socket:{normalizedDefNum}");
                        }
                        
                        m_services.GetLogger().LogDebug("Ingame - ChangeWeevilDef: {Def}", dbDef);
                        
                        // there is no need to sync anything as this is sent before re-joining any room
                        weevilData.m_weevilDef.SetValue(dbDef);
                    });
                    break;
                }
                default:
                {
                    m_services.GetLogger().LogWarning("Unknown ingame command: {Command} - {Args}", message.m_command.ToString(), string.Join(" ", reader.ReadToEnd()));
                    break;
                }
            }
        }
        
        private async Task<Room?> TryJoinRoom(string roomName)
        {
            var user = GetUser();
                        
            var newRoom = await user.m_zone.GetRoom(roomName);
            if (roomName.StartsWith("nest_"))
            {
                var system = m_services.GetActorSystem();

                var nestUser = roomName.Substring("nest_".Length);
                if (!await TryJoinNestRoom(system, newRoom))
                {
                     // attempt to salvage the situation
                    system.Root.Send(user.GetUserData<WeevilData>().GetUserAddress(), new SocketActor.KickFromNest(nestUser));
                    return null;
                }
                
                return newRoom;
            }

            if (newRoom == null)
            {
                throw new InvalidDataException($"trying to join a room that doesn't exist: {roomName}");
            }
            await user.MoveTo(newRoom);
            return newRoom;
        }
        
        private async Task<bool> TryJoinNestRoom(ActorSystem system, Room? newRoom)
        {
            if (newRoom?.GetDataAs<NestRoom>() is not {} nestRoom)
            {
                return false;
            }
                
            bool success;
            try
            {
                success = await system.Root.RequestAsync<bool>(nestRoom.m_nest, new NestActor.Join(GetUser(), newRoom));
            } catch (DeadLetterException)
            {
                success = false;
            } catch (TimeoutException)
            {
                success = false;
            }
            return success;
        }

        private void HandleInGameCommand_Action(ref StrReader reader)
        {
            var action = new ClientAction();
            action.Deserialize(ref reader);
            
            if (!Enum.IsDefined(typeof(EWeevilAction), action.m_actionID))
            {
                throw new InvalidDataException($"invalid action id: {action.m_endPoseID}");
            }
                    
            if (action.m_endPoseID != -1 && !Enum.IsDefined(typeof(EWeevilAction), action.m_endPoseID))
            {
                throw new InvalidDataException($"invalid end pose id: {action.m_endPoseID}");
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

        private void HandleInGameCommand_Action_UpdateVars(ClientAction action, WeevilData weevil)
        {
            m_services.GetLogger().LogDebug("Ingame - Action: {Action} - {Args}", (EWeevilAction)action.m_actionID, action.m_extraParams);

            var extraParamsReader = new StrReader(action.m_extraParams, ',', WeevilStrParser.s_instance);
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
                    
                    weevil.m_r.SetValue(weevil.CalculateNewDirection(flyUpTube.m_xFinal, flyUpTube.m_zFinal));
                    weevil.m_x.SetValue(flyUpTube.m_xFinal);
                    weevil.m_y.SetValue(flyUpTube.m_yDest);
                    weevil.m_z.SetValue(flyUpTube.m_zFinal);
                    break;
                }
                case EWeevilAction.SKATE:
                {
                    throw new InvalidDataException("client sent SKATE action - not allowed");
                }
                case EWeevilAction.GET_SPUN:
                {
                    var getSpun = new GetSpunAction();
                    getSpun.Deserialize(ref extraParamsReader);
                    
                    if (getSpun.m_vr < 15 || getSpun.m_vr > 30)
                    {
                        throw new InvalidDataException("GET_SPUN velocity out of range");
                    }
                    
                    // todo: not really any point syncing r..
                    // this is for blue diamond only
                    
                    break;
                }
                case EWeevilAction.SLIDE_OUT:
                {
                    var slideOut = new SlideOutAction();
                    slideOut.Deserialize(ref extraParamsReader);
                    
                    if (slideOut.m_slideFactor > 0.94 || slideOut.m_slideFactor < 0.85)
                    {
                        // sanity
                        // new slime pool uses 0.85
                        // old uses 0.94
                        throw new InvalidDataException("invalid slide out");
                    }
                    
                    var x = (double)slideOut.m_xStart;
                    var z = (double)slideOut.m_zStart;
                    
                    var vx = (double) slideOut.m_vx;
                    var vz = (double) slideOut.m_vz;

                    for (var i = 0; i < 100; i++)
                    {
                        // todo: replace with non-iterative
                        
                        x += vx;
                        z += vz;
                        vx *= slideOut.m_slideFactor;
                        vz *= slideOut.m_slideFactor;
                        if (vx < 0.5 && vx > -0.5 && vz < 0.5 && vz > -0.5)
                        {
                            break;
                        }
                    }
                    
                    weevil.m_x.SetValue(x);
                    weevil.m_y.SetValue(slideOut.m_yStart);
                    weevil.m_z.SetValue(z);
                    weevil.m_r.SetValue(weevil.CalculateNewDirection(
                        slideOut.m_xStart, slideOut.m_zStart, 
                        slideOut.m_xStart + slideOut.m_vx, slideOut.m_zStart + slideOut.m_vz));
                    break;
                }
                // todo: which other actions need this
                
                
                // =========================================
                // =========================================
                // VALIDATE ONLY
                // =========================================
                // =========================================
                
                case EWeevilAction.WALK:
                {
                    throw new InvalidDataException("client sent WALK action - not allowed");
                }
                case EWeevilAction.JUMP:
                {
                    var jump = new SpinAction();
                    jump.Deserialize(ref extraParamsReader);

                    if (jump.m_level < 1 || jump.m_level > 5)
                    {
                        throw new InvalidDataException("invalid jump level");
                    }
                    break;
                }
                case EWeevilAction.SIT_IN_CAR:
                case EWeevilAction.EXIT_CAR:
                {
                    var exitCar = new ExitCarAction();
                    exitCar.Deserialize(ref extraParamsReader);
                    // todo: validate
                    
                    break;
                }
                // todo: THROW (validate only)
                case EWeevilAction.SPIN1:
                case EWeevilAction.SPIN2:
                {
                    var spin = new SpinAction();
                    spin.Deserialize(ref extraParamsReader);
                    
                    if (spin.m_level < 1 || spin.m_level > 5)
                    {
                        throw new InvalidDataException("invalid spin level");
                    }
                    break;
                }
                case EWeevilAction.JIGGLE:
                {
                    var jiggle = new JiggleAction();
                    jiggle.Deserialize(ref extraParamsReader);
                    
                    if (jiggle.m_armMode < 1 || jiggle.m_armMode > 3)
                    {
                        throw new InvalidDataException("invalid jiggle arm mode");
                    }
                    break;
                }
                case EWeevilAction.HOLD_TRAY:
                {
                    throw new InvalidDataException("client HOLD_TRAY WALK action - not allowed");
                }
                case EWeevilAction.HOLD_ITEM:
                {
                    var holdItem = new HoldItemAction();
                    holdItem.Deserialize(ref extraParamsReader);
                    
                    if (holdItem.m_itemType < 1 || holdItem.m_itemType > 10)
                    {
                        throw new InvalidDataException("invalid hold item type");
                    }
                    break;
                }
                case EWeevilAction.ADD_ITEM_TO_TRAY:
                {
                    var addItemToTray = new AddItemToTrayAction();
                    addItemToTray.Deserialize(ref extraParamsReader);
                    
                    if (addItemToTray.m_trayType != 1 && addItemToTray.m_trayType != 2)
                    {
                        throw new InvalidDataException("invalid tray type");
                    }
                    
                    // todo: validate
                    break;
                }
                case EWeevilAction.TELEPORT_OUT:
                {
                    var teleportOut = new TeleportOutAction();
                    teleportOut.Deserialize(ref extraParamsReader);
                    
                    break;
                }
                case EWeevilAction.FLY_OUT:
                {
                    var flyOut = new FlyOutAction();
                    flyOut.Deserialize(ref extraParamsReader);
                    
                    break;
                }
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
                if (action.m_actionID == (int)EWeevilAction.SQUAT)
                {
                    // invalid action sent by halloween_bedroom
                    // idk what its trying to do... sit on the bed?
                    return;
                }
                
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
        
        private void HandleSfsSetUserVars(ReadOnlySpan<char> body)
        {
            var setUvars = XmlReadBuffer.ReadStatic<SetUserVarsRequest>(body, CDataMode.On);
            m_services.GetLogger().LogDebug("Sfs - SetUserVars: {Vars}", setUvars.m_vars.m_vars);
            
            // we need to handle y as the client uses it to get down from chairs and such
            
            var yVar = setUvars.m_vars.m_vars.SingleOrDefault(x => x.m_name == "y");
            if (yVar.m_value != null && float.TryParse(yVar.m_value, out var newYValue))
            {
                var yValue = float.Parse(yVar.m_value);
                
                m_taskQueue.Enqueue(() =>
                {
                    var user = GetUser();
                    var weevil = user.GetUserData<WeevilData>();
                    
                    weevil.m_y.SetValue(yValue);
                    
                    return ValueTask.CompletedTask;
                });
            }
        }
    }
}