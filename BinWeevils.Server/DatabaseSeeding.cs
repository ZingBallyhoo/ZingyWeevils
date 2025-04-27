using System.Text.Json;
using BinWeevils.Database;
using BinWeevils.Protocol.Json;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server
{
    public class DatabaseSeeding
    {
        private readonly IConfiguration m_configuration;
        private readonly WeevilDBContext m_dbContext;
        
        public DatabaseSeeding(IConfiguration configuration, WeevilDBContext dbContext)
        {
            m_configuration = configuration;
            m_dbContext = dbContext;
        }
        
        public async Task Seed()
        {
            // todo: optional:
            await m_dbContext.Database.EnsureDeletedAsync(); // reset
            
            if (!await m_dbContext.Database.EnsureCreatedAsync())
            {
                // already exists, assume seeding is up to date...
                return;
            }
            
            var itemSql = await File.ReadAllTextAsync(Path.Combine("Data", "itemType.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(itemSql);
            
            var apparelSql = await File.ReadAllTextAsync(Path.Combine("Data", "apparelTypes.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(apparelSql);
            
            await SeedPalettes();
        }
        
        private async Task SeedPalettes()
        {
            var jsonText = await File.ReadAllTextAsync(Path.Combine("Data", "getAvailablePalettes.php"));
            var json = JsonSerializer.Deserialize<AvailablePalettes>(jsonText);

            foreach (var palettePair in json!.m_palette)
            {
                var paletteId = palettePair.Key;

                for (var index = 0; index < palettePair.Value.Count; index++)
                {
                    var color = palettePair.Value[index];
                    await m_dbContext.m_paletteEntries.AddAsync(new PaletteEntryDB
                    {
                        m_paletteID = paletteId,
                        m_index = index,
                        
                        m_color = color
                    });
                }

            }
            
            await m_dbContext.SaveChangesAsync();
        }
    }
}