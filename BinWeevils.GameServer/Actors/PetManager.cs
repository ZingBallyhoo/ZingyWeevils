using System.Text.RegularExpressions;
using BinWeevils.GameServer.PolyType;
using BinWeevils.GameServer.Rooms;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.KeyValue;
using BinWeevils.Protocol.Str.Pet;
using BinWeevils.Protocol.XmlMessages;
using Proto;
using StackXML.Str;
using Stl.Collections;

namespace BinWeevils.GameServer.Actors
{
    public partial class PetManager : IActor
    {
        private class Pet 
        {
            public PetDefVar m_def;
            public PetStateVar m_state;
        }
        
        public required WeevilSocketServices m_services;
        public required WeevilData m_weevilData;
        private Dictionary<uint, Pet> m_pets = [];
        private uint? m_equippedPet;
        
        public record GetNestVarsRequest();
        public record PetNotification(string command, IStrClass data, bool inNest);
        
        public async Task ReceiveAsync(IContext context)
        {
            // mount (in nest) sequence
            // uvar: petDef
            // rvar: petIDs
            // uvar: petState
            
            // dismount (in nest) sequence:
            // rvar: petIDs
            // rvar: petStateN
            // uvar: petDef = ""
            
            // login sequence:
            // petDefN
            // petStateN
            // petIDs
            
            // returnToNest sequence:
            // petStateN
            // PET_GO_TO_NEST
            
            switch (context.Message)
            {
                case Started:
                {
                    var pets = await m_services.GetPets(m_weevilData.m_idx.GetValue());
                    m_weevilData.m_myPetIDs.AddRange(pets.Keys);
                    m_weevilData.m_myPetNames.AddRange(pets.Values.Select(x => x.m_name));
                    
                    m_pets = pets.ToDictionary(x => x.Key, x => new Pet
                    {
                        m_def = x.Value,
                        m_state = new PetStateVar
                        {
                            // just a general "In the nest" state until the client starts sending updates
                            m_locID = -5,
                            m_pose = EPetAction.DEFAULT,
                            m_x = 50,
                            m_y = 7.2,
                            m_z = 160,
                            m_r = -103
                        }
                    });
                    
                    break;
                }
                case SetUserVarsRequest userVars:
                {
                    await HandleUserVars(context, userVars);
                    break;
                }
                case SetRoomVarsRequest roomVars:
                {
                    HandleRoomVars(context, roomVars);
                    break;
                }
                case ClientPetJoinNestLoc joinNestLoc:
                {
                    ValidatePetID(joinNestLoc.m_shared.m_petID);
                    var petInNest = await ValidateBroadcastSwitch(joinNestLoc.m_shared.m_petID, joinNestLoc.m_broadcastSwitch);
                    if (!petInNest)
                    {
                        throw new InvalidDataException("how could a pet outside of the nest join a nest loc...?");
                    }
                    
                    context.Send(m_weevilData.GetUserAddress(), new PetNotification(Modules.PET_MODULE_JOIN_NEST_LOC, joinNestLoc.m_shared, petInNest));
                    
                    var pet = m_pets[joinNestLoc.m_shared.m_petID];
                    UpdatePetState(joinNestLoc.m_shared.m_petID, pet.m_state with
                    {
                        m_locID = joinNestLoc.m_shared.m_locID,
                        m_x = joinNestLoc.m_shared.m_x,
                        m_y = joinNestLoc.m_shared.m_y,
                        m_z = joinNestLoc.m_shared.m_z,
                        m_r = joinNestLoc.m_shared.m_r,
                    });
                    break;
                }
                case ClientPetSetNestDoor setNestDoor:
                {
                    ValidatePetID(setNestDoor.m_shared.m_petID);
                    var petInNest = await ValidateBroadcastSwitch(setNestDoor.m_shared.m_petID, setNestDoor.m_broadcastSwitch);
                    if (!petInNest)
                    {
                        throw new InvalidDataException("how could a pet outside of the nest set a nest door...?");
                    }
                    
                    context.Send(m_weevilData.GetUserAddress(), new PetNotification(Modules.PET_MODULE_SET_NEST_DOOR, setNestDoor.m_shared, petInNest));
                    break;
                }
                case ClientPetExpression expression:
                {
                    ValidatePetID(expression.m_shared.m_petID);
                    if (!Enum.IsDefined(typeof(EPetExpression), expression.m_shared.m_expressionID))
                    {
                        throw new InvalidDataException($"invalid pet expression: {expression.m_shared.m_expressionID}");
                    }
                    var petInNest = await ValidateBroadcastSwitch(expression.m_shared.m_petID, expression.m_broadcastSwitch);
                    
                    context.Send(m_weevilData.GetUserAddress(), new PetNotification(Modules.PET_MODULE_EXPRESSION, expression.m_shared, petInNest));
                    GameServerObservability.s_petExpressionsSent.Add(1);
                    break;
                }
                case ClientPetAction action:
                {
                    ValidatePetID(action.m_petID);
                    if (!Enum.IsDefined(typeof(EPetAction), action.m_actionID))
                    {
                        throw new InvalidDataException($"invalid pet action: {action.m_actionID}");
                    }
                    var petInNest = await ValidateBroadcastSwitch(action.m_petID, action.m_broadcastSwitch);
                    
                    // todo: validate extra params
                    
                    if (action.m_stateVars != "-1") 
                    {
                        UpdatePetState(action.m_petID, ParsePetState(action.m_stateVars));
                    } 
                    
                    // sitting isn't synchronised, we can help
                    // todo: when outside of a nest, clients ignore non-JUMP_ON poses
                    var pet = m_pets[action.m_petID];
                    if (action.m_actionID == (int)EPetAction.SIT && pet.m_state.m_pose != EPetAction.JUMP_ON)
                    {
                        UpdatePetState(action.m_petID, pet.m_state with
                        {
                            m_pose = EPetAction.SIT
                        });
                    }
                    
                    var serverAction = new ServerPetAction
                    {
                        m_petID = action.m_petID,
                        m_actionID = action.m_actionID,
                        m_extraParams = action.m_extraParams
                    };
                    context.Send(m_weevilData.GetUserAddress(), new PetNotification(Modules.PET_MODULE_ACTION, serverAction, petInNest));
                    GameServerObservability.s_petActionsSent.Add(1);
                    break;
                }
                case ClientPetGoHome petGoHome:
                {
                    if (m_equippedPet != petGoHome.m_petID)
                    {
                        throw new InvalidDataException("sending an unequipped pet home");
                    }
                    
                    var newPetIDs = ParsePetIDsList(petGoHome.m_petIDsVar);
                    if (!newPetIDs.SetEquals(m_pets.Keys.ToHashSet()))
                    {
                        throw new InvalidDataException("petgohome ids should be every pet");
                    }
                    
                    var pet = m_pets[petGoHome.m_petID];
                    var newState = ParsePetState(petGoHome.m_petState);
                    if (newState != pet.m_state)
                    {
                        throw new InvalidDataException("client should have already sent new state (rvar) at this point");
                    }
                    
                    UnequipPet();
                    context.Send(m_weevilData.GetUserAddress(), new PetNotification(Modules.PET_MODULE_RETURN_TO_NEST, new ServerPetGoHome
                    {
                        m_petDef = pet.m_def.ToString(),
                        m_petState = pet.m_state.ToString(),
                    }, true));
                    break;
                }
                
                case GetNestVarsRequest:
                {
                    var petsInRoom = m_pets.Keys.Where(x => x != m_equippedPet).ToList();
                    
                    var bag = new VarBag();
                    bag.UpdateVar(Var.String("petIDs", string.Join(',', petsInRoom)));
                    foreach (var petPair in m_pets)
                    {
                        bag.UpdateVar(Var.String($"petDef{petPair.Key}", petPair.Value.m_def.ToString()));
                        bag.UpdateVar(Var.String($"petState{petPair.Key}", petPair.Value.m_state.ToString()));
                    }
                    
                    context.Respond(bag);
                    break;
                }
            }
        }
        
        private async ValueTask<bool> ValidateBroadcastSwitch(uint petID, byte val) 
        {
            var petInNest = petID != m_equippedPet || await UserInNest();
            if (petInNest != (val == 0))
            {
                throw new InvalidDataException("client disagrees on pet being in nest or not");
            }
            return petInNest;
        }
        
        private async ValueTask<bool> UserInNest()
        {
            var room = await m_weevilData.m_user.GetRoomOrNull();
            if (room == null) return false;
            
            var nestRoom = room.GetDataAs<NestRoom>();
            if (nestRoom == null) return false;
            
            return nestRoom.m_ownerWeevil.m_user.m_name == m_weevilData.m_user.m_name;
        }
        
        private async ValueTask HandleUserVars(IContext context, SetUserVarsRequest setUserVars)
        {
            var petDefVar = setUserVars.m_vars.m_vars.SingleOrDefault(x => x.m_name == "petDef");
            if (petDefVar.m_value != null)
            {
                PetDefVar? parsedPetDef = null;
                if (!string.IsNullOrEmpty(petDefVar.m_value))
                {
                    parsedPetDef = ParsePetDef(petDefVar.m_value);
                }
                
                await UserPetDefChanged(parsedPetDef);
            }
            
            var petStateVar = setUserVars.m_vars.m_vars.SingleOrDefault(x => x.m_name == "petState");
            if (petStateVar.m_value != null)
            {
                var parsedState = ParsePetState(petStateVar.m_value);
                UserPetStateChanged(parsedState);
            }
        }
        
        private void HandleRoomVars(IContext context, SetRoomVarsRequest setRoomVars)
        {
            // if (setRoomVars.m_room != todo) return;
            
            var petIDsVar = setRoomVars.m_varList.m_roomVars.SingleOrDefault(x => x.m_name == "petIDs");
            if (petIDsVar.m_value != null)
            {
                var idsInNest = ParsePetIDsList(petIDsVar.m_value);
                if (idsInNest.Count != m_pets.Count && idsInNest.Count != m_pets.Count-1)
                {
                    throw new InvalidDataException($"invalid pet-in-nest count: {idsInNest}. pet count: {m_pets.Count}");
                }
                
                if (m_equippedPet != null && idsInNest.Contains(m_equippedPet.Value))
                {
                    UnequipPet();
                }
            }

            foreach (var roomVar in setRoomVars.m_varList.m_roomVars)
            {
                if (roomVar.m_name.StartsWith("petDef"))
                {
                    var match = PetDefRoomVarRegex.Match(roomVar.m_name);
                    if (!match.Success)
                    {
                        throw new InvalidDataException($"invalid room pet def var name: {roomVar.m_name}");
                    }
                    var petID = uint.Parse(match.Groups[1].ValueSpan);
                    ValidatePetRoomUpdate(petID);
                    
                    ParsePetDef(roomVar.m_value);
                    // nothing to do, we already manage this
                } else if (roomVar.m_name.StartsWith("petState"))
                {
                    var match = PetStateRoomVarRegex.Match(roomVar.m_name);
                    if (!match.Success)
                    {
                        throw new InvalidDataException($"invalid room pet state var name: {roomVar.m_name}");
                    }
                    
                    var petID = uint.Parse(match.Groups[1].ValueSpan);
                    
                    var state = ParsePetState(roomVar.m_value);
                    if (m_equippedPet == petID)
                    {
                        // sending home...
                        ValidatePetID(petID);
                    } else
                    {
                        ValidatePetRoomUpdate(petID);
                    }
                    UpdatePetState(petID, state);
                }
            }
        }
        
        private HashSet<uint> ParsePetIDsList(string text)
        {
            if (text.Length == 0) return [];
            return text.Split(',').Select(uint.Parse).ToHashSet();
        }
        
        [GeneratedRegex(@"^petDef(\d+)$")]
        private partial Regex PetDefRoomVarRegex { get; }
        
        [GeneratedRegex(@"^petState(\d+)$")]
        private partial Regex PetStateRoomVarRegex { get; }
        
        private async Task UserPetDefChanged(PetDefVar? def)
        {
            // todo: validate we are in our nest
            
            if (!await UserInNest())
            {
                if (def == null)
                {
                    throw new InvalidDataException("can only unequip pets while in nest");
                }
                if (def.Value.m_id != m_equippedPet)
                {
                    throw new InvalidDataException("can only equip pets while in nest");
                    // client sends when re-mounting
                }
                return;
            }
            
            if (def != null)
            {
                if (m_equippedPet != null)
                {
                    throw new InvalidDataException("pet already equipped");
                }
                
                // pet added to us
                
                m_equippedPet = def.Value.m_id;
                m_weevilData.AddPet(def.Value);
                
                // predict client state
                UpdatePetState(m_equippedPet.Value, new PetStateVar
                {
                    m_pose = EPetAction.JUMP_ON
                });
            } else
            {
                UnequipPet();
            }
        }
        
        private void UnequipPet()
        {
            if (m_equippedPet == null) return;
            
            // pet removed from us, placed back in nest
            m_weevilData.RemovePet();
            m_equippedPet = null;
        }
        
        private void UserPetStateChanged(PetStateVar state)
        {
            if (m_equippedPet == null /*&& m_pets.Count > 1*/)
            {
                throw new InvalidDataException("ambiguous pet state update");
            }
            
            UpdatePetState(m_equippedPet.Value, state);
        }
        
        private void UpdatePetState(uint id, PetStateVar state)
        {
            if (state.m_locID == 0)
            {
                if (m_equippedPet != id)
                {
                    throw new InvalidDataException("pet state without loc, but pet is not equipped");
                }
                state.m_locID = m_weevilData.m_locID;
            }
            
            m_pets[id].m_state = state;
            if (m_equippedPet == id)
            {
                m_weevilData.SetPetState(state);
            }
        }
        
        private void ValidatePetID(uint petID)
        {
            if (!m_pets.ContainsKey(petID))
            {
                throw new InvalidDataException("referenced pet does not exist");
            }
        }
        
        private void ValidatePetRoomUpdate(uint petID)
        {
            if (m_equippedPet == petID)
            {
                throw new InvalidDataException("trying to update pet in room, but pet is equipped");
            }
            ValidatePetID(petID);
        }
        
        private PetDefVar ParsePetDef(ReadOnlySpan<char> str)
        {
            var parsedDef = KeyValueDeserializer.Deserialize<PetDefVar>(str);
            if (m_pets[parsedDef.m_id].m_def != parsedDef)
            {
                throw new InvalidDataException("client-sent pet def doesn't match db");
            }
            
            return parsedDef;
        }
        
        private static PetStateVar ParsePetState(ReadOnlySpan<char> str)
        {
            var parsedState = KeyValueDeserializer.Deserialize<PetStateVar>(str);
            if (!parsedState.m_pose.IsValidPose()) 
            {
                throw new InvalidDataException($"invalid pet pose: {parsedState.m_pose}");
            }
            return parsedState;
        }
    }
}