using BinWeevils.Protocol.Sql;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class WeevilDBContext : IdentityDbContext<WeevilAccount>
    {
        public DbSet<WeevilDB> m_weevilDBs { get; set; }
        public DbSet<ItemType> m_itemTypes { get; set; }
        public DbSet<NestDB> m_nests { get; set; }
        
        public WeevilDBContext(DbContextOptions<WeevilDBContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<WeevilDB>(b =>
            {
                b.ToTable("Weevil");
            });
            
            modelBuilder.Entity<NestRoomDB>(b =>
            {
                // (constraint)
                b.HasAlternateKey(p => new { p.m_nestID, p.m_type });
            });
            
            modelBuilder.Entity<NestItemDB>(b =>
            {
                b.HasOne(x => x.m_placedItem)
                 .WithOne(x => x.m_item)
                 .IsRequired(false);
            });
            
            modelBuilder.Entity<NestPlacedItemDB>(b =>
            {
                // (constraint)
                // todo: we have to use a "shadow" column because using the foreign key in an alt-key causes
                // ef core to promote it to non-null...
                b.HasAlternateKey(p => new { p.m_roomID, p.m_currentPos, p.m_placedOnFurnitureIDShadow, p.m_spotOnFurniture });
                
                // todo: not sure which is right yet.
                //b.HasMany(x => x.m_ornaments)
                // .WithOne(x => x.m_placedOnFurniture)
                // .OnDelete(DeleteBehavior.Cascade)
                // .IsRequired(false);
                
                //b.HasOne(x => x.m_placedOnFurniture)
                // .WithMany(x => x.m_ornaments)
                // .IsRequired(false);
                // .OnDelete(DeleteBehavior.Cascade);*/
            });
        }
        
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // todo: efcore doesnt respect DataMemberName for enums (expression based...)
            // (so it would write uppercase names)
            
            configurationBuilder.Properties<ItemCategory>()
                .HaveConversion<string>();
            configurationBuilder.Properties<ItemCurrency>()
                .HaveConversion<string>();
            configurationBuilder.Properties<ItemInternalCategory>()
                .HaveConversion<string>();
            configurationBuilder.Properties<ItemShopType>()
                .HaveConversion<string>();
        }
        
        public async Task<WeevilDB> CreateWeevil(WeevilCreateParams createParams)
        {
            var now = DateTime.Now;;
            
            var dbWeevil = new WeevilDB
            {
                m_name = createParams.m_name,
                m_createdAt = now,
                m_lastLogin = now,
                m_weevilDef = createParams.m_weevilDef,
                m_food = 75,
                m_fitness = 50,
                m_happiness = 75,
                m_xp = 0,
                m_lastAcknowledgedLevel = 1,
                m_mulch = 2000,
                m_dosh = 0
            };
            
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
                m_itemType = await m_itemTypes
                    .SingleAsync(x => x.m_configLocation == "o_egg")
            });
            await SaveChangesAsync();
            
            return dbWeevil;
        }
        
        private async Task<NestDB> CreateNest(string nestDef)
        {
            throw new NotImplementedException();
        }
        
        /*public async Task<bool> TakeMulch(IQueryable<WeevilDB> weevil, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
            
            var rowsUpdated = await weevil
                .Where(x => x.m_mulch >= amount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch - amount));
            return rowsUpdated == 1;
        }*/
    }
    
    public class WeevilCreateParams
    {
        public required string m_name;
        public required ulong m_weevilDef;
        
        public string? m_nestDef;
    }
}