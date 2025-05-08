using ArcticFox.RPC.AmfGateway;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Controllers
{
    public class PetAmfService
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly PetsSettings m_settings;
        
        public PetAmfService(WeevilDBContext dbContext, IOptionsSnapshot<PetsSettings> settings)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
        }
        
        public Task<int> GetPetCount(AmfGatewayContext context, GetUserPetCountRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetAmfService.GetPetCount");
            activity?.SetTag("userID", request.m_userID);

            if (context.m_httpContext.User.Identity!.Name != request.m_userID)
            {
                throw new InvalidDataException("trying to buy a pet for somebody else");
            }
            
            return m_dbContext.m_pets.CountAsync(x => x.m_owner.m_name == request.m_userID);
        }
        
        private Task<int> GetPetCount(uint idx)
        {
            return m_dbContext.m_pets.CountAsync(x => x.m_ownerIdx == idx);
        }
        
        public Task<int> ValidatePetName(AmfGatewayContext context, ValidatePetNameRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetAmfService.ValidatePetName");
            activity?.SetTag("name", request.m_name);
            
            return Task.FromResult(ValidatePetName(request.m_name));
        }
        
        private int ValidatePetName(string name)
        {
            if (!m_settings.Enabled) return 0;
            if (name.Length > m_settings.MaxNameLength) return 0;
            return 1;
        }
        
        public async Task<bool> BuyPet(AmfGatewayContext context, BuyPetRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetAmfService.BuyPet");
            activity?.SetTag("name", request.m_name);
            activity?.SetTag("bodyColor", request.m_bodyColor);
            activity?.SetTag("antenna1Color", request.m_antenna1Color);
            activity?.SetTag("antenna2Color", request.m_antenna2Color);
            activity?.SetTag("eye1Color", request.m_eye1Color);
            activity?.SetTag("eye2Color", request.m_eye2Color);
            activity?.SetTag("bowlItemTypeID", request.m_bowlItemTypeID);
            activity?.SetTag("bedColor", request.m_bedColor);
            
            if (context.m_httpContext.User.Identity!.Name != request.m_userID)
            {
                throw new InvalidDataException("trying to buy a pet for somebody else");
            }
            
            if (ValidatePetName(request.m_name) != 1)
            {
                return false;
            }
            
            if (!m_settings.Colors.Contains(request.m_bodyColor) || 
                !m_settings.Colors.Contains(request.m_antenna1Color) ||
                !m_settings.Colors.Contains(request.m_antenna2Color) ||
                !m_settings.Colors.Contains(request.m_eye1Color) ||
                !m_settings.Colors.Contains(request.m_eye2Color))
            {
                throw new InvalidDataException("invalid part color");
            }
            
            if (!m_settings.ItemColors.Contains(request.m_bedColor) ||
                !m_settings.BowlItemTypes.Contains(request.m_bowlItemTypeID))
            {
                throw new InvalidDataException("invalid item color");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .Select(x => new
                {
                    x.m_idx,
                    m_nestID = x.m_nest.m_id
                })
                .SingleAsync();
            
            // todo: subtract price
            
            var bowlItem = new NestItemDB
            {
                m_itemTypeID = request.m_bowlItemTypeID,
                m_nestID = dto.m_nestID
            };
            var bedItem = new NestItemDB
            {
                m_itemTypeID = (await m_dbContext.FindItemByConfigName("f_petBasket2"))!.Value,
                m_nestID = dto.m_nestID,
                m_color = ItemColor.Parse($"0x{request.m_bedColor:X}", null)
            };
            await m_dbContext.m_nestItems.AddAsync(bowlItem);
            await m_dbContext.m_nestItems.AddAsync(bedItem);
            
            await m_dbContext.m_pets.AddAsync(new PetDB
            {
                m_ownerIdx = dto.m_idx,
                m_name = request.m_name,
                m_bodyColor = request.m_bodyColor,
                m_antenna1Color = request.m_antenna1Color,
                m_antenna2Color = request.m_antenna2Color,
                m_eye1Color = request.m_eye1Color,
                m_eye2Color = request.m_eye2Color,
                m_bowlItem = bowlItem,
                m_bedItem = bedItem,
                m_skills = 
                    Enumerable.Range(0, (int)EPetSkill.COUNT)
                    .Where(x =>
                    {
                        var skill = (EPetSkill)x;
                        var clientManaged = skill switch
                        {
                            EPetSkill.CALL => true,
                            EPetSkill.GO_THERE => true,
                            EPetSkill.WEEVIL_THROW_BALL => true,
                            EPetSkill.STOP_JUGGLING => true,
                            _ => false,
                        };
                        
                        return !clientManaged;
                    }).Select(x => new PetSkillDB
                    {
                        m_skillID = (EPetSkill)x
                    })
                    .ToList()
            });
            
            await m_dbContext.SaveChangesAsync();
            
            if (await GetPetCount(dto.m_idx) > m_settings.MaxUserPets)
            {
                throw new InvalidDataException("buying a pet would go over the owned pet limit");
            }
            await transaction.CommitAsync();
            
            return true; // todo
        }
    }
}