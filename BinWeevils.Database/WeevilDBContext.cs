using BinWeevils.Protocol;
using BinWeevils.Protocol.Sql;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class WeevilDBContext : IdentityDbContext<WeevilAccount>
    {
        public DbSet<WeevilDB> m_weevilDBs { get; set; }
        public DbSet<BuddyRecordDB> m_buddyRecords { get; set; }
        public DbSet<CompletedTaskDB> m_completedTasks { get; set; }
        public DbSet<RewardedTaskDB> m_rewardedTasks { get; set; }

        // public DbSet<NestDB> m_nests { get; set; }
        public DbSet<NestItemDB> m_nestItems { get; set; }
        public DbSet<NestGardenItemDB> m_nestGardenItems { get; set; }
        public DbSet<NestRoomDB> m_nestRooms { get; set; }
        public DbSet<NestPlacedItemDB> m_nestPlacedItems { get; set; }
        public DbSet<NestPlacedGardenItemDB> m_nestPlacedGardenItems { get; set; }
        public DbSet<BusinessDB> m_businesses { get; set; }
        
        public DbSet<ItemType> m_itemTypes { get; set; }
        public DbSet<ApparelType> m_apparelTypes { get; set; }
        public DbSet<PaletteEntryDB> m_paletteEntries { get; set; }
        public DbSet<SeedType> m_seedTypes { get; set; }
        
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
            });
            
            modelBuilder.Entity<BuddyRecordDB>(b =>
            {
                b.ToTable("BuddyRecordDB");
            });
            
            modelBuilder.Entity<CompletedTaskDB>(b =>
            {
                b.ToTable("CompletedTaskDB");
            });
            modelBuilder.Entity<RewardedTaskDB>(b =>
            {
                b.ToTable("RewardedTaskDB");
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
                
                b.HasOne(x => x.m_placedItem)
                 .WithOne(x => x.m_item)
                 .IsRequired(false);
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
                
                b.HasOne(x => x.m_placedItem)
                    .WithOne(x => x.m_item)
                    .IsRequired(false);
            });
            modelBuilder.Entity<NestPlacedGardenItemDB>(b =>
            {
                b.ToTable("NestPlacedGardenItemDB");
            });
            
            modelBuilder.Entity<BusinessDB>(b =>
            {
                b.ToTable("BusinessDB");
            });
            
            modelBuilder.Entity<ItemType>(b =>
            {
                b.HasIndex(x => x.m_configLocation);
            });
            modelBuilder.Entity<PaletteEntryDB>(b =>
            {
                b.ToTable("PaletteEntryDB");
                b.HasIndex(x => new { x.m_paletteID, x.m_color });
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
        
        public async Task<WeevilDB> CreateWeevil(WeevilCreateParams createParams)
        {
            var now = DateTime.UtcNow;
            
            var dbWeevil = new WeevilDB
            {
                m_name = createParams.m_name,
                m_createdAt = now,
                m_lastLogin = now,
                m_weevilDef = createParams.m_weevilDef,
                m_food = 75,
                m_fitness = 50,
                m_happiness = 75,
                m_lastAcknowledgedLevel = 1,
                m_mulch = 20000,
                m_dosh = 0
            };
            dbWeevil.m_xp = WeevilLevels.GetXpForLevel(dbWeevil.m_lastAcknowledgedLevel);
            
            NestDB nest;
            if (createParams.m_nestDef != null)
            {
                nest = await CreateNest(createParams.m_nestDef);
            } else
            {
                nest = NestDB.Empty();
            }
            dbWeevil.m_nest = nest;
            
            await m_weevilDBs.AddAsync(dbWeevil);
            await SaveChangesAsync();
            
            //var test = await m_itemTypes.Where(x => x.m_configLocation == "f_shelf1").ToArrayAsync();
            //Console.Out.WriteLine($"{test.Length}");
            
            dbWeevil.m_nest.m_items.Add(new NestItemDB
            {
                // todo: there are 2 f_shelf1... one with color one without
                
                m_itemType = await m_itemTypes
                    .Where(x => x.m_paletteID == -1)
                    .SingleAsync(x => x.m_configLocation == "f_shelf1")
            });
            dbWeevil.m_nest.m_items.Add(new NestItemDB
            {
                m_itemTypeID = (await FindItemByConfigName("o_egg"))!.Value
            });
            dbWeevil.m_nest.m_gardenItems.Add(new NestGardenItemDB
            {
                m_itemTypeID = (await FindItemByConfigName("deckChairRed"))!.Value,
                m_placedItem = new NestPlacedGardenItemDB
                {
                    m_x = -69,
                    m_z = 417,
                    m_room =  dbWeevil.m_nest.m_rooms.Single(x => x.m_type == ENestRoom.Garden)
                }
            });
            await SaveChangesAsync();
            
            return dbWeevil;
        }
        
        private async Task<NestDB> CreateNest(string nestDef)
        {
            throw new NotImplementedException();
        }
        
        public async Task<uint?> FindItemByConfigName(string configName)
        {
            return await m_itemTypes
                .Where(x => x.m_configLocation == configName)
                .Select(x => x.m_itemTypeID)
                .SingleOrDefaultAsync();
        }
    }
    
    public class WeevilCreateParams
    {
        public required string m_name;
        public required ulong m_weevilDef;
        
        public string? m_nestDef;
    }
}