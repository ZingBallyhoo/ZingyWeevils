using System.Net.Mime;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form.Pet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackXML.Str;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class PetController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly PetsSettings m_settings;
        
        public PetController(WeevilDBContext dbContext, IOptionsSnapshot<PetsSettings> settings)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
        }
        
        [StructuredFormPost("pets/defs")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetPetDefsResponse> GetPetDefs([FromBody] GetPetDefsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetController.GetPetDefs");
            activity?.SetTag("userID", request.m_userID);

            if (ControllerContext.HttpContext.User.Identity!.Name != request.m_userID)
            {
                throw new InvalidDataException("trying to get someone else's pet defs");
            }
            
            if (!m_settings.Enabled)
            {
                return new GetPetDefsResponse();
            }
            
            var pets = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .SelectMany(x => x.m_pets)
                .Select(x => new DelimitedPetDef
                {
                    m_id = x.m_id,
                    m_name = x.m_name,
                    m_bedID = x.m_bedItem.m_id,
                    m_bowlID = x.m_bowlItem.m_id,
                    m_bodyColor = x.m_bodyColor,
                    m_antenna1Color = x.m_antenna1Color,
                    m_antenna2Color = x.m_antenna2Color,
                    m_eye1Color = x.m_eye1Color,
                    m_eye2Color = x.m_eye2Color,
                    m_fuel = x.m_fuel,
                    m_mentalEnergy = x.m_mentalEnergy,
                    m_health = x.m_health,
                    m_fitness = x.m_fitness,
                    m_experience = x.m_experience,
                    m_nameHash = "todo"
                }).ToListAsync();
            
            return new GetPetDefsResponse
            {
                m_resultDefs = string.Join('|', pets.Select(x => x.AsString(';')))
            };
        }
        
        [StructuredFormPost("php/updatePetStats.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task UpdatePetStats([FromBody] UpdatePetStatsRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetController.UpdatePetStats");
            activity?.SetTag("petID", request.m_petID);
            activity?.SetTag("mentalEnergy", request.m_mentalEnergy);
            activity?.SetTag("fuel", request.m_fuel);
            activity?.SetTag("health", request.m_health);
            activity?.SetTag("fitness", request.m_fitness);
            activity?.SetTag("experience", request.m_experience);
            
            // when overeating the game will send an update without clamping values
            // (and can send negative health...)
            request.m_health = Math.Max(request.m_health, (byte)2);
            
            if (request.m_mentalEnergy > 100 ||
                request.m_fuel > 100 ||
                request.m_health > 100 ||
                request.m_fitness > 100 ||
                request.m_fitness < 35)
            {
                throw new InvalidDataException("pet stat out of range");
            }
            
            var rowsUpdated = await m_dbContext.m_pets
                .Where(x => x.m_id == request.m_petID)
                .Where(x => x.m_owner.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_experience <= request.m_experience) // can only go up
                .Where(x => (request.m_experience - x.m_experience) <= 25) // can't go up too much :)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mentalEnergy, request.m_mentalEnergy)
                    .SetProperty(x => x.m_fuel, request.m_fuel)
                    .SetProperty(x => x.m_health, request.m_health)
                    .SetProperty(x => x.m_fitness, request.m_fitness)
                    .SetProperty(x => x.m_experience, request.m_experience)
                );
            
            if (rowsUpdated == 0)
            {
                throw new Exception("invalid update pet stats request");
            }
        }
        
        [StructuredFormPost("php/getPetSkills.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetPetSkillsResponse> GetPetSkills([FromBody] GetPetSkillsRequest request) 
        {
            using var activity = ApiServerObservability.StartActivity("PetController.GetPetSkills");
            activity?.SetTag("petID", request.m_petID);
            
            if (!await m_dbContext.m_pets.AnyAsync(x => x.m_id == request.m_petID && x.m_owner.m_name == ControllerContext.HttpContext.User.Identity!.Name))
            {
                throw new Exception("invalid pet skills request");
            }
            
            var skills = await m_dbContext.m_pets
                .Where(x => x.m_id == request.m_petID)
                .SelectMany(x => x.m_skills)
                .Select(x => new DelimitedPetSkill 
                {
                    m_skillID = (uint)x.m_skillID,
                    m_obedience = x.m_obedience,
                    m_skillLevel = x.m_skillLevel
                })
                .ToArrayAsync();
            
            return new GetPetSkillsResponse
            {
                m_resultSkills = string.Join('|', skills.Select(x => x.AsString(';')))
            };
        }
        
        [StructuredFormPost("php/updatePetSkill.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task UpdatePetSkill([FromBody] UpdatePetSkillRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetController.UpdatePetSkill");
            activity?.SetTag("petID", request.m_petID);
            activity?.SetTag("skillID", request.m_skillID);
            activity?.SetTag("obedience", request.m_obedience);
            activity?.SetTag("skillLevel", request.m_skillLevel);
            
            if (request.m_skillID.IsClientManaged())
            {
                // for some reason the client sends updates for these
                // even though their stats is set at load time by the client
                return;
            }
            
            if (request.m_obedience > 105 || 
                request.m_skillLevel > 100 || 
                request.m_skillLevel < 0)
            {
                throw new InvalidDataException("pet skill value out of range");
            }
            
            var rowsUpdated = await m_dbContext.m_petSkills
                .Where(x => x.m_petID == request.m_petID)
                .Where(x => x.m_skillID == request.m_skillID)
                .Where(x => x.m_pet.m_owner.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_skillLevel <= request.m_skillLevel) // can only go up
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_obedience, request.m_obedience)
                    .SetProperty(x => x.m_skillLevel, request.m_skillLevel)
                );
            
            if (rowsUpdated == 0)
            {
                throw new Exception("invalid update pet skill request");
            }
        }
        
        [HttpPost("php/getPetJugglingSkills.php")]
        public string GetJugglingSKills()
        {
            return "result=1;1;300,0,0,0,0,9,0;10;99|10;1;40004000300,0,0,0,0,9,0;12;99";
        }
        
        [HttpPost("php/getMyPetFoodStock.php")]
        public string GetPetFoodStock()
        {
            return "result=9";
        }
    }
}