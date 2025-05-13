using System.Text.Json;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Json;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Services
{
    public class DatabaseSeeding
    {
        private readonly DatabaseSettings m_settings;
        private readonly WeevilDBContext m_dbContext;
        
        public DatabaseSeeding(IOptionsSnapshot<DatabaseSettings> settings, WeevilDBContext dbContext)
        {
            m_settings = settings.Value;
            m_dbContext = dbContext;
        }
        
        public async Task Seed()
        {
            /*if (m_settings.ResetOnStartup)
            {
                await m_dbContext.Database.EnsureDeletedAsync(); 
                
                if (!await m_dbContext.Database.EnsureCreatedAsync())
                {
                    // already exists, assume seeding is up to date...
                    return;
                }
            } else
            {
                await m_dbContext.Database.MigrateAsync();
            }*/
            
            await m_dbContext.Database.EnsureDeletedAsync(); 
            await m_dbContext.Database.EnsureCreatedAsync();
            
            var itemSql = await File.ReadAllTextAsync(Path.Combine("Data", "itemType.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(itemSql.Replace("INSERT INTO", "INSERT OR REPLACE INTO"));
            
            var apparelSql = await File.ReadAllTextAsync(Path.Combine("Data", "apparelTypes.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(apparelSql.Replace("INSERT INTO", "INSERT OR REPLACE INTO"));
            
            var seedSql = await File.ReadAllTextAsync(Path.Combine("Data", "seeds.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(seedSql.Replace("INSERT INTO", "INSERT OR REPLACE INTO"));
            
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
            await SeedJugglingTricks();
        }
        
        private async Task SeedPalettes()
        {
            if (await m_dbContext.m_paletteEntries.AnyAsync()) 
            {
                // todo: upsert needs support for complex types
                return;
            }
            
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
        
        private async Task SeedJugglingTricks()
        {
            var tricksJsonText = await File.ReadAllTextAsync(Path.Combine("Data", "jugglingTricks.json"));
            var tricks = JsonSerializer.Deserialize<List<JsonJugglingTrick>>(tricksJsonText)!;
            foreach (var jsonJugglingTrick in tricks)
            {
                await m_dbContext.m_jugglingTricks.Upsert(new JugglingTrickDB
                {
                    m_id = jsonJugglingTrick.m_id,
                    m_numBalls = jsonJugglingTrick.m_numBalls,
                    m_name = jsonJugglingTrick.m_name,
                    m_difficulty = jsonJugglingTrick.m_difficulty,
                    m_pattern = jsonJugglingTrick.m_pattern
                }).RunAsync();
            }
            
            await m_dbContext.SaveChangesAsync();
        }
    }
}