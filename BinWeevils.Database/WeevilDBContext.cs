using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class WeevilDBContext : IdentityDbContext<WeevilAccount>
    {
        public DbSet<WeevilDB> m_weevilDBs { get; set; }
        
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
            modelBuilder.Entity<WalletDB>(b =>
            {
                b.ToTable("Wallet");
            });
        }
    }
}