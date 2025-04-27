using System.Text.Json;
using System.Text.RegularExpressions;
using BinWeevils.Database;
using BinWeevils.Protocol.Sql;
using BinWeevils.Server;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Tests
{
    public class SeedDatabaseFixture : IAsyncLifetime
    {
        public readonly WeevilDBContext m_dbContext;
        public readonly Dictionary<string, uint> m_prices = [];
        
        public SeedDatabaseFixture()
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeevilDBContext>();
            optionsBuilder.UseSqlite("Filename=seedCompare.sqlite");
            
            m_dbContext = new WeevilDBContext(optionsBuilder.Options);
        }
        
        public async Task InitializeAsync()
        {
            var seeding = new DatabaseSeeding(null!, m_dbContext);
            await seeding.Seed();
            
            foreach (Match match in Regex.Matches(await File.ReadAllTextAsync(@"D:\re\bw\archive\lb.binweevils.com\gardenshop\fetch"), """
                fileName="([^"]*)".+ price="(\d+)"
                """))
            {
                m_prices[match.Groups[1].Value] = uint.Parse(match.Groups[2].Value);
            }
            
            // todo: only has 103 prices out of 191
        }

        public async Task DisposeAsync()
        {
            await m_dbContext.DisposeAsync();
        }
    }
    
    public class SeedCompare : IClassFixture<SeedDatabaseFixture>
    {
        private const string BASE_DIR = @"D:\re\bw\archive\other\plant_configs";
        private readonly SeedDatabaseFixture m_fixture;
        
        public SeedCompare(SeedDatabaseFixture fixture)
        {
            m_fixture = fixture;
        }
        
        [Theory]
        [MemberData(nameof(GetPlantTypes))]
        public async Task Validate(string fileName)
        {
            var config = JsonSerializer.Deserialize<ScrapedPlantInfo>(await File.ReadAllTextAsync(Path.Combine(BASE_DIR, fileName)))!;
                
            var dbSeed = await m_fixture.m_dbContext.m_seedTypes
                .SingleAsync(x => x.m_fileName == config.m_fileName);
                
            Assert.Equal(config.m_name.TrimEnd(), dbSeed.m_name);
            Assert.Equal(config.m_category, dbSeed.m_category);
            Assert.Equal(config.m_mulch, dbSeed.m_mulchYield);
            Assert.Equal(config.m_xp, dbSeed.m_xpYield);
            Assert.Equal(config.m_growTime, dbSeed.m_growTime);
            Assert.Equal(config.m_cycleTime, dbSeed.m_cycleTime);
            Assert.Equal(config.m_radius, dbSeed.m_radius);
            
            if (m_fixture.m_prices.TryGetValue(config.m_fileName, out var expectedPrice))
            {
                Assert.Equal(expectedPrice, dbSeed.m_price);
            }
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