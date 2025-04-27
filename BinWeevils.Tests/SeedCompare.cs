using System.Text.Json;
using BinWeevils.Database;
using BinWeevils.Protocol.Sql;
using BinWeevils.Server;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Tests
{
    public class SeededDatabaseFixture : IAsyncLifetime
    {
        public WeevilDBContext m_dbContext;
        
        public async Task InitializeAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeevilDBContext>();
            optionsBuilder.UseSqlite("Filename=seedCompare.sqlite");
            
            m_dbContext = new WeevilDBContext(optionsBuilder.Options);
            var seeding = new DatabaseSeeding(null!, m_dbContext);
            await seeding.Seed();
        }

        public async Task DisposeAsync()
        {
            await m_dbContext.DisposeAsync();
        }
    }
    
    public class SeedCompare : IClassFixture<SeededDatabaseFixture>
    {
        private const string BASE_DIR = @"D:\re\bw\archive\other\plant_configs";
        private WeevilDBContext m_dbContext;
        
        public SeedCompare(SeededDatabaseFixture fixture)
        {
            m_dbContext = fixture.m_dbContext;
        }
        
        [Theory]
        [MemberData(nameof(GetPlantTypes))]
        public async Task Validate(string fileName)
        {
            var config = JsonSerializer.Deserialize<ScrapedPlantInfo>(await File.ReadAllTextAsync(Path.Combine(BASE_DIR, fileName)))!;
                
            var dbPlant = await m_dbContext.m_seedTypes.SingleAsync(x => x.m_fileName == config.m_fileName);
                
            Assert.Equal(config.m_name.TrimEnd(), dbPlant.m_name);
            Assert.Equal(config.m_category, dbPlant.m_category);
            Assert.Equal(config.m_mulch, dbPlant.m_mulchYield);
            Assert.Equal(config.m_xp, dbPlant.m_xpYield);
            Assert.Equal(config.m_growTime, dbPlant.m_growTime);
            Assert.Equal(config.m_cycleTime, dbPlant.m_cycleTime);
            Assert.Equal(config.m_radius, dbPlant.m_radius);
        }
        
        public static IEnumerable<object[]> GetPlantTypes()
        {
            return Directory.GetFiles(BASE_DIR, "*.json")
                .Select(Path.GetFileName)
                .Select(x => new object[] {x});
        }
        
        private class ScrapedPlantInfo
        {
            public SeedCategory m_category { get; set; }
            public uint m_cycleTime { get; set; }
            public string m_fileName { get; set; }
            public uint m_growTime { get; set; }
            public uint m_mulch { get; set; }
            public string m_name { get; set; }
            public uint m_radius { get; set; }
            public uint m_xp { get; set; }
        }
    }
}