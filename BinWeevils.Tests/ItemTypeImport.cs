using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Tests
{
    public class ItemDatabaseFixture : IAsyncLifetime
    {
        private readonly ServiceProvider m_provider;
        public readonly WeevilDBContext m_dbContext;
        
        public ItemDatabaseFixture()
        {
            var builder = new ServiceCollection();
            builder.AddOptions<DatabaseSettings>().Configure(h =>
            {
                h.ConnectionString = "Filename=itemImport.sqlite";
                h.ResetOnStartup = true;
            });
            builder.AddDbContext<WeevilDBContext>((provider, options) =>
            {
                var settings = provider.GetRequiredService<IOptionsSnapshot<DatabaseSettings>>().Value;
                options.UseSqlite(settings.ConnectionString);
            });
            builder.AddSingleton<DatabaseSeeding>();
            
            m_provider = builder.BuildServiceProvider();
            m_dbContext = m_provider.GetRequiredService<WeevilDBContext>();
        }
        
        public async Task InitializeAsync()
        {
            var seeding = m_provider.GetRequiredService<DatabaseSeeding>();
            await seeding.Seed();
        }

        public async Task DisposeAsync()
        {
            await m_provider.DisposeAsync();
        }
    }
    
    public class ItemTypeImport : IClassFixture<ItemDatabaseFixture>
    {
        private readonly ItemDatabaseFixture m_fixture;
        
        public ItemTypeImport(ItemDatabaseFixture fixture)
        {
            m_fixture = fixture;
        }
        
        [Fact]
        public async Task ValidateDefaultColors()
        {
            await foreach (var itemDto in m_fixture.m_dbContext.m_itemTypes
                .Where(x => x.m_defaultHexColor != "-1") // meaning choose from palette...
                .Select(x => new
                {
                    x.m_itemTypeID,
                    x.m_category,
                    x.m_configLocation,
                    x.m_paletteID,
                    x.m_defaultHexColor
                })
                .AsAsyncEnumerable())
            {
                await m_fixture.m_dbContext.ValidateShopItemColor(itemDto.m_defaultHexColor, itemDto.m_paletteID);
            }
        }
    }
}