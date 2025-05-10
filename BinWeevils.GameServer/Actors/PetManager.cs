using System.Text.RegularExpressions;
using BinWeevils.GameServer.PolyType;
using BinWeevils.Protocol;
using BinWeevils.Protocol.KeyValue;
using BinWeevils.Protocol.Str.Pet;
using BinWeevils.Protocol.XmlMessages;
using Proto;
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
                            // just a general "In the nest" state until the client starts sending upadtes
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
                    HandleUserVars(context, userVars);
                    break;
                }
                case SetRoomVarsRequest roomVars:
                {
                    HandleRoomVars(context, roomVars);
                    break;
                }
                case ClientPetAction petAction:
                {
                    break;
                }
            }
        }
        
        private void HandleUserVars(IContext context, SetUserVarsRequest setUserVars)
        {
            var petDefVar = setUserVars.m_vars.m_vars.SingleOrDefault(x => x.m_name == "petDef");
            if (petDefVar.m_value != null)
            {
                PetDefVar? parsedPetDef = null;
                if (!string.IsNullOrEmpty(petDefVar.m_value))
                {
                    parsedPetDef = ParsePetDef(petDefVar.m_value);
                }
                
                UserPetDefChanged(parsedPetDef);
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
                var idsInNest = petIDsVar.m_value.Split(',').Select(uint.Parse).ToHashSet();
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
                    ValidatePetRoomUpdate(petID);
                    UpdatePetState(petID, ParsePetState(roomVar.m_value));
                }
            }
        }
        
        [GeneratedRegex(@"^petDef(\d+)$")]
        private partial Regex PetDefRoomVarRegex { get; }
        
        [GeneratedRegex(@"^petState(\d+)$")]
        private partial Regex PetStateRoomVarRegex { get; }
        
        private void UserPetDefChanged(PetDefVar? def)
        {
            // todo: validate we are in our nest
            
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
            m_pets[m_equippedPet!.Value].m_state = state;
            if (m_equippedPet == id)
            {
                m_weevilData.SetPetState(state);
            }
        }
        
        private void ValidatePetRoomUpdate(uint petID)
        {
            if (m_equippedPet == petID)
            {
                throw new InvalidDataException("trying to update pet in room, but pet is equipped");
            }
            if (!m_pets.ContainsKey(petID))
            {
                throw new InvalidDataException("referenced pet does not exist");
            }
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