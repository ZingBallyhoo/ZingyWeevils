using BinWeevils.Protocol.Sql;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class WeevilDBContext : IdentityDbContext<WeevilAccount>
    {
        public DbSet<WeevilDB> m_weevilDBs { get; set; }
        public DbSet<ItemType> m_itemTypes { get; set; }
        
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
    }
}