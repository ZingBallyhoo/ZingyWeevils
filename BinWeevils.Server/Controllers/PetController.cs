using System.Net.Mime;
using BinWeevils.Database;
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
                m_resultDefs = string.Join('|', pets.Select(x => x.AsString(';')).Append("0;h;0;0;0;0;0;0;0;100;100;100;100;99;h"))
            };
        }
        
        [HttpPost("php/getPetSkills.php")]
        public string GetPetSkills() 
        {
            return $"result={string.Join('|', Enumerable.Range(1, 17).Select(x => $"{x};100;0"))}";
        }
        
        [HttpPost("php/getPetJugglingSkills.php")]
        public string GetJugglingSKills()
        {
            return "result=1;1;300,0,0,0,0,9,0;10;99|10;1;40004000300,0,0,0,0,9,0;12;99";
        }
    }
}