using System.Text.Json;
using System.Text.RegularExpressions;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Json;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Services
{
    public partial class DatabaseSeeding
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
            if (m_settings.ResetOnStartup)
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
            }
            
            var itemSql = await File.ReadAllTextAsync(Path.Combine("Data", "itemType.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(CreateUpsert(itemSql));
            
            var apparelSql = await File.ReadAllTextAsync(Path.Combine("Data", "apparelTypes.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(CreateUpsert(apparelSql));
            
            var seedSql = await File.ReadAllTextAsync(Path.Combine("Data", "seeds.sql"));
            await m_dbContext.Database.ExecuteSqlRawAsync(CreateUpsert(seedSql));
            
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
        
        [GeneratedRegex(@" \((.*)\) VALUES")]
        private static partial Regex InsertValuesRegex { get; }
        
        private static string CreateUpsert(string rawSql)
        {
            var reader = new StringReader(rawSql);
            var headerLine = reader.ReadLine()!;
            
            var columns = new List<string>();
            var match = InsertValuesRegex.Match(headerLine);
            foreach (var quotedColumn in match.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!quotedColumn.StartsWith('`') || !quotedColumn.EndsWith('`'))
                {
                    throw new InvalidDataException($"column name isn't quoted: {quotedColumn}");
                }
                
                var column = quotedColumn.Substring(1, quotedColumn.Length-2);
                columns.Add(column);
            }
            
            if (!rawSql.EndsWith(';')) 
            {
                throw new InvalidDataException("sql should end with \";\"");
            }
            var writer = new StringWriter();
            writer.WriteLine(rawSql.AsSpan(0, rawSql.Length-1));
            writer.WriteLine($"ON CONFLICT({columns.First()}) DO UPDATE SET");
            var first = true;
            foreach (var column in columns.Skip(1))
            {
                if (first) first = false;
                else writer.WriteLine(',');
                writer.Write($"    {column}=EXCLUDED.{column}");
            }
            writer.WriteLine(';');
            
            return writer.ToString();
        }
        
        private async Task SeedPalettes()
        {
            // todo: upsert needs support for complex types
            await m_dbContext.m_paletteEntries.ExecuteDeleteAsync();
            
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