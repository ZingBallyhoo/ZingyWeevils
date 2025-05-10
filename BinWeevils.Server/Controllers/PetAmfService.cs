using ArcticFox.RPC.AmfGateway;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Controllers
{
    public class PetAmfService
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly PetInitializer m_petInitializer;
        private readonly PetsSettings m_settings;
        
        public PetAmfService(WeevilDBContext dbContext, PetInitializer petInitializer, IOptionsSnapshot<PetsSettings> settings)
        {
            m_dbContext = dbContext;
            m_petInitializer = petInitializer;
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
            // todo: validate allowed characters
            return 1;
        }
        
        public async Task<uint> BuyPet(AmfGatewayContext context, BuyPetRequest request)
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
                return 0;
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
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == dto.m_idx)
                .Where(x => x.m_mulch >= m_settings.Cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - m_settings.Cost));
            if (rowsUpdated == 0)
            {
                // can't afford
                return 0;
            }
            
            await m_petInitializer.CreatePet(new PetCreateParams
            {
                m_ownerIdx = dto.m_idx,
                m_nestID = dto.m_nestID,
                m_name = request.m_name,
                m_antenna1Color = request.m_antenna1Color,
                m_antenna2Color = request.m_antenna2Color,
                m_eye1Color = request.m_eye1Color,
                m_eye2Color = request.m_eye2Color,
                m_bodyColor = request.m_bodyColor,
                m_itemParams = new PetNewItemParams 
                {
                    m_bowlItemTypeID = request.m_bowlItemTypeID,
                    m_bedColor = request.m_bedColor
                }
            });
            await transaction.CommitAsync();
            
            return m_settings.Cost;
        }
        
        public async Task<uint> BuyPetFood(AmfGatewayContext context, BuyPetFoodRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PetAmfService.BuyPetFood");
            activity?.SetTag("type", request.m_type);
            
            if (context.m_httpContext.User.Identity!.Name != request.m_userID)
            {
                throw new InvalidDataException("trying to buy pet food for somebody else");
            }
            
            if (!m_settings.FoodPacks.TryGetValue(request.m_type, out var foodPack))
            {
                throw new InvalidDataException("unknown food pack");
            }
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .Where(x => x.m_mulch >= foodPack.Cost)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - foodPack.Cost)
                    .SetProperty(x => x.m_petFoodStock, x => x.m_petFoodStock + foodPack.Feeds));
            if (rowsUpdated == 0)
            {
                return 0;
            }
            
            return foodPack.Cost;
        }
    }
}