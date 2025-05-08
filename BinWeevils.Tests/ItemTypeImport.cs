using BinWeevils.Common.Database;
using BinWeevils.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Tests
{
    public class ItemDatabaseFixture : IAsyncLifetime
    {
        public readonly WeevilDBContext m_dbContext;
        
        public ItemDatabaseFixture()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeevilDBContext>();
            optionsBuilder.UseSqlite("Filename=itemImport.sqlite");
            
            m_dbContext = new WeevilDBContext(optionsBuilder.Options);
        }
        
        public async Task InitializeAsync()
        {
            var seeding = new DatabaseSeeding(null!, m_dbContext);
            await seeding.Seed();
        }

        public async Task DisposeAsync()
        {
            await m_dbContext.DisposeAsync();
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