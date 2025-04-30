using System.Text.Json;
using BinWeevils.Database;
using BinWeevils.Protocol.Json;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Services
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
            
            var seedSql = await File.ReadAllTextAsync(Path.Combine("Data", "seeds.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(seedSql);
            
            // todo: modern hats seem to break the game
            // why?
            await m_dbContext.m_apparelTypes
                .Where(x => x.m_id >= 100)
                .ExecuteDeleteAsync();
            
            // todo: core we are using doesn't support ceilings
            // this is the easiest way of getting rid of them
            await m_dbContext.m_itemTypes
                .Where(x => x.m_configLocation.StartsWith("ceiling_"))
                .ExecuteDeleteAsync();
            
            await SeedPalettes();
        }
        
        private async Task SeedPalettes()
        {
            var jsonText = await File.ReadAllTextAsync(Path.Combine("Data", "getAvailablePalettes.php"));
            var json = JsonSerializer.Deserialize<AvailablePalettes>(jsonText);
            
            // simplify all of our code by creating a null palette
            await m_dbContext.m_paletteEntries.AddAsync(new PaletteEntryDB
            {
                m_paletteID = 0,
                m_index = 0,
                
                m_colorString = "0,0,0",
                m_color = new ItemColor()
            });

            foreach (var palettePair in json!.m_palette)
            {
                var paletteId = palettePair.Key;

                for (var index = 0; index < palettePair.Value.Count; index++)
                {
                    var color = palettePair.Value[index];
                    if (!ItemColor.TryParse(color, null, out var parsedColor))
                    {
                        throw new InvalidDataException($"unable to parse palette color: {color}");
                    }
                    
                    await m_dbContext.m_paletteEntries.AddAsync(new PaletteEntryDB
                    {
                        m_paletteID = paletteId,
                        m_index = index,
                        
                        m_colorString = color,
                        m_color = parsedColor
                    });
                }

            }
            
            await m_dbContext.SaveChangesAsync();
        }
    }
}