using BinWeevils.Protocol.Sql;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Common.Database
{
    public class WeevilDBContext : IdentityDbContext<WeevilAccount>
    {
        public DbSet<WeevilDB> m_weevilDBs { get; set; }
        public DbSet<WeevilSpecialMoveDB> m_weevilSpecialMoves { get; set; }
        public DbSet<WeevilGamePlayedDB> m_weevilGamesPlayed { get; set; }
        public DbSet<BuddyRecordDB> m_buddyRecords { get; set; }
        public DbSet<BuddyMessageDB> m_buddyMesssages { get; set; }
        public DbSet<IgnoreRecordDB> m_ignoreRecords { get; set; }
        public DbSet<CompletedTaskDB> m_completedTasks { get; set; }
        public DbSet<RewardedTaskDB> m_rewardedTasks { get; set; }
        
        public DbSet<PetDB> m_pets { get; set; }
        public DbSet<PetSkillDB> m_petSkills { get; set; }
        public DbSet<PetJugglingTrickDB> m_petJugglingTricks { get; set; }

        public DbSet<NestDB> m_nests { get; set; }
        public DbSet<NestItemDB> m_nestItems { get; set; }
        public DbSet<NestGardenItemDB> m_nestGardenItems { get; set; }
        public DbSet<NestSeedItemDB> m_nestGardenSeeds { get; set; }
        public DbSet<NestRoomDB> m_nestRooms { get; set; }
        public DbSet<NestPlacedItemDB> m_nestPlacedItems { get; set; }
        public DbSet<NestPlacedGardenItemDB> m_nestPlacedGardenItems { get; set; }
        public DbSet<NestPlantDB> m_nestPlacedSeeds { get; set; }
        public DbSet<BusinessDB> m_businesses { get; set; }
        
        public DbSet<ItemType> m_itemTypes { get; set; }
        public DbSet<ApparelType> m_apparelTypes { get; set; }
        public DbSet<PaletteEntryDB> m_paletteEntries { get; set; }
        public DbSet<SeedType> m_seedTypes { get; set; }
        public DbSet<JugglingTrickDB> m_jugglingTricks { get; set; }
        
        public WeevilDBContext(DbContextOptions<WeevilDBContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<WeevilDB>(b =>
            {
                b.ToTable("WeevilDB");
                b.HasIndex(x => x.m_name);
                b.HasMany(x => x.m_ignoredWeevils)
                 .WithOne(x => x.m_forWeevil);
            });
            modelBuilder.Entity<WeevilSpecialMoveDB>(b =>
            {
                b.ToTable("WeevilSpecialMoveDB");
            });
            modelBuilder.Entity<WeevilGamePlayedDB>(b =>
            {
                b.ToTable("WeevilGamePlayedDB");
            });
            
            modelBuilder.Entity<BuddyRecordDB>(b =>
            {
                b.ToTable("BuddyRecordDB");
            });
            modelBuilder.Entity<BuddyMessageDB>(b =>
            {
                b.ToTable("BuddyMessageDB");
            });
            modelBuilder.Entity<IgnoreRecordDB>(b =>
            {
                b.ToTable("IgnoreRecordDB");
            });
            
            modelBuilder.Entity<CompletedTaskDB>(b =>
            {
                b.ToTable("CompletedTaskDB");
            });
            modelBuilder.Entity<RewardedTaskDB>(b =>
            {
                b.ToTable("RewardedTaskDB");
            });
            
            modelBuilder.Entity<PetDB>(b =>
            {
                b.ToTable("PetDB");
                // todo: cascade behavior?
                // i think currently it will delete the bed/bowl
                // which i guess could be fine... otherwise it would become an orphan
            });
            modelBuilder.Entity<PetSkillDB>(b =>
            {
                b.ToTable("PetSkillDB");
            });
            modelBuilder.Entity<PetJugglingTrickDB>(b =>
            {
                b.ToTable("PetJugglingTrickDB");
            });
            
            modelBuilder.Entity<NestRoomDB>(b =>
            {
                b.ToTable("NestRoomDB");

                // (constraint)
                b.HasAlternateKey(p => new { p.m_nestID, p.m_type });
                
                b.ComplexProperty(x => x.m_color);
                
                b.HasOne(x => x.m_business)
                    .WithOne(x => x.m_room)
                    .IsRequired(false);
            });
            
            modelBuilder.Entity<NestItemDB>(b =>
            {
                b.ToTable("NestItemDB");
                b.ComplexProperty(x => x.m_color);
                
                b.HasOne(x => x.m_placedItem)
                 .WithOne(x => x.m_item)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<NestPlacedItemDB>(b =>
            {
                b.ToTable("NestPlacedItemDB");
                
                // (constraint)
                // this constraint does two things
                // 1 - prevent placing multiple ornaments in the same spot
                // 2 - prevent placing multiple frames of the same item
                // validating more than that is hard because it requires full collision checks
                b.HasAlternateKey(p => new { p.m_roomID, p.m_posIdentity, p.m_spotOnFurniture, p.m_posAnimationFrame });
                
                b.HasMany(x => x.m_ornaments)
                 .WithOne(x => x.m_placedOnFurniture)
                 .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired(false);
            });
            
            modelBuilder.Entity<NestGardenItemDB>(b =>
            {
                b.ToTable("NestGardenItemDB");
                b.ComplexProperty(x => x.m_color);
                
                b.HasOne(x => x.m_placedItem)
                    .WithOne(x => x.m_item)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<NestPlacedGardenItemDB>(b =>
            {
                b.ToTable("NestPlacedGardenItemDB");
            });
            
            modelBuilder.Entity<NestSeedItemDB>(b =>
            {
                b.ToTable("NestSeedItemDB");
                
                b.HasOne(x => x.m_placedItem)
                    .WithOne(x => x.m_item)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<NestPlantDB>(b =>
            {
                b.ToTable("NestPlantDB");
            });
            
            modelBuilder.Entity<BusinessDB>(b =>
            {
                b.ToTable("BusinessDB");
                b.ComplexProperty(x => x.m_playList);
            });
            
            modelBuilder.Entity<ItemType>(b =>
            {
                b.HasIndex(x => x.m_configLocation);
            });
            modelBuilder.Entity<PaletteEntryDB>(b =>
            {
                b.ToTable("PaletteEntryDB");
                b.ComplexProperty(x => x.m_color);
                b.HasIndex(x => new { x.m_paletteID, x.m_colorString });
            });
            modelBuilder.Entity<JugglingTrickDB>(b =>
            {
                b.ToTable("JugglingTrickDB");
                b.HasIndex(t => new { t.m_pattern });
            });
        }
        
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // todo: efcore doesnt respect DataMemberName for enums (expression based...)
            // (so it would write uppercase names)
            
            configurationBuilder.Properties<ItemCurrency>()
                .HaveConversion<string>();
            configurationBuilder.Properties<ItemInternalCategory>()
                .HaveConversion<string>();
            configurationBuilder.Properties<ItemShopType>()
                .HaveConversion<string>();
        }
        
        public async Task<uint?> FindItemByConfigName(string configName)
        {
            return await m_itemTypes
                .Where(x => x.m_configLocation == configName)
                .Select(x => x.m_itemTypeID)
                .SingleOrDefaultAsync();
        }
        
        public async Task<uint?> FindSeedByConfigName(string configName)
        {
            return await m_seedTypes
                .Where(x => x.m_fileName == configName)
                .Select(x => x.m_id)
                .SingleOrDefaultAsync();
        }
        
        public async Task SetNestUpdatedNoConcurrency(uint nestID) 
        {
            await m_nests
                .Where(x => x.m_id == nestID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lastUpdated, DateTime.UtcNow));
        }

        public async Task<ItemColor> ValidateShopItemColor(string str, int paletteID) 
        {
            if (!ItemColor.TryParse(str, null, out var parsedColor))
            {
                throw new InvalidDataException("invalid color string");
            }
            
            if (paletteID == -1)
            {
                if (parsedColor.m_r != 0 || parsedColor.m_g != 0 || parsedColor.m_b != 0)
                {
                    throw new InvalidDataException("no color expected");
                }
                return parsedColor;
            }
            
            var valid = await m_paletteEntries
                .Where(x => x.m_paletteID == paletteID)
                .AnyAsync(x => 
                    x.m_color.m_r  == parsedColor.m_r &&
                    x.m_color.m_g  == parsedColor.m_g &&
                    x.m_color.m_b  == parsedColor.m_b);
            if (!valid)
            {
                throw new InvalidDataException("color not allowed by palette");
            }
            
            return parsedColor;
        }
    }
    
    public class WeevilCreateParams
    {
        public required string m_name;
        public required ulong m_weevilDef;
        
        public int? m_mulch;
        public uint? m_xp;
        public string? m_nestDef;
        public PetCreateParams? m_pet;
    }
}