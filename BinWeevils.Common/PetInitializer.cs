using BinWeevils.Common.Database;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Common
{
    public class PetInitializer
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly PetsSettings m_settings;
        
        public PetInitializer(WeevilDBContext dbContext, IOptionsSnapshot<PetsSettings> settings)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
        }
        
        public async Task CreatePet(PetCreateParams createParams)
        {
            if (!m_settings.Colors.Contains(createParams.m_bodyColor) || 
                !m_settings.Colors.Contains(createParams.m_antenna1Color) ||
                !m_settings.Colors.Contains(createParams.m_antenna2Color) ||
                !m_settings.Colors.Contains(createParams.m_eye1Color) ||
                !m_settings.Colors.Contains(createParams.m_eye2Color))
            {
                throw new InvalidDataException("invalid part color");
            }
            
            NestItemDB bowlItem;
            NestItemDB bedItem;
            if (createParams.m_itemParams is PetNewItemParams newItems)
            {
                if (!m_settings.ItemColors.Contains(newItems.m_bedColor) ||
                    !m_settings.BowlItemTypes.Contains(newItems.m_bowlItemTypeID))
                {
                    throw new InvalidDataException("invalid item color");
                }
                
                bowlItem = new NestItemDB
                {
                    m_itemTypeID = newItems.m_bowlItemTypeID,
                    m_nestID = createParams.m_nestID
                };
                bedItem = new NestItemDB
                {
                    m_itemTypeID = (await m_dbContext.FindItemByConfigName(m_settings.BedItem))!.Value,
                    m_nestID = createParams.m_nestID,
                    m_color = ItemColor.Parse($"0x{newItems.m_bedColor:X}", null)
                };
                await m_dbContext.m_nestItems.AddAsync(bowlItem);
                await m_dbContext.m_nestItems.AddAsync(bedItem);
            } else if (createParams.m_itemParams is PetExistingItemParams existingItems)
            {
                bowlItem = await m_dbContext.m_nestItems.Where(x => x.m_nestID == createParams.m_nestID && x.m_id == existingItems.m_bowlItemID).SingleAsync();
                bedItem = await m_dbContext.m_nestItems.Where(x => x.m_nestID == createParams.m_nestID && x.m_id == existingItems.m_bedItemID).SingleAsync();
            } else
            {
                throw new NotImplementedException($"unknown item params: {createParams.m_itemParams}");
            }
            
            var skills = 
                Enumerable.Range(0, (int)EPetSkill.COUNT)
                    .Where(x =>
                    {
                        var skill = (EPetSkill)x;
                        return !skill.IsClientManaged();
                    }).Select(x => new PetSkillDB
                    {
                        m_skillID = (EPetSkill)x
                    })
                    .ToList();
            
            await m_dbContext.m_pets.AddAsync(new PetDB
            {
                m_ownerIdx = createParams.m_ownerIdx,
                m_name = createParams.m_name,
                m_adoptedAt = DateTime.UtcNow,
                
                m_bodyColor = createParams.m_bodyColor,
                m_antenna1Color = createParams.m_antenna1Color,
                m_antenna2Color = createParams.m_antenna2Color,
                m_eye1Color = createParams.m_eye1Color,
                m_eye2Color = createParams.m_eye2Color,
                
                m_mentalEnergy = createParams.m_mentalEnergy,
                m_fuel = createParams.m_fuel,
                m_health = createParams.m_health,
                m_fitness = createParams.m_fitness,
                m_experience = createParams.m_experience,
                
                m_bowlItem = bowlItem,
                m_bedItem = bedItem,
                m_skills = skills
            });
            await m_dbContext.SaveChangesAsync();
            
            if (await GetPetCount(createParams.m_ownerIdx) > m_settings.MaxUserPets)
            {
                throw new InvalidDataException("adding this pet would go over the owned pet limit");
            }
        }
        
        private Task<int> GetPetCount(uint idx)
        {
            return m_dbContext.m_pets.CountAsync(x => x.m_ownerIdx == idx);
        }
    }
    
    public class PetCreateParams
    {
        public required uint m_ownerIdx;
        public required uint m_nestID;
        public required string m_name;
        public required uint m_bodyColor;
        public required uint m_antenna1Color;
        public required uint m_antenna2Color;
        public required uint m_eye1Color;
        public required uint m_eye2Color;
        
        public required object m_itemParams;
        
        public byte m_mentalEnergy = 50; // todo: decide default value. isn't shown ingame
        public byte m_fuel = 60;
        public byte m_health = 60;
        public byte m_fitness = 40;
        public uint m_experience = 0;
    }
    
    public class PetExistingItemParams
    {
        public uint m_bedItemID;
        public uint m_bowlItemID;
    }
    
    public class PetNewItemParams
    {
        public uint m_bedColor;
        public uint m_bowlItemTypeID;
    }
}