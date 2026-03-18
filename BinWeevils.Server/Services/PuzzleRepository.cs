using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcticFox.PolyType.FormEncoded;
using BinWeevils.Protocol.Form;

namespace BinWeevils.Server.Services
{
    public class PuzzleRepository
    {
        public PuzzleDefinition[] WordSearches { get; }
        
        public PuzzleRepository(IConfiguration configuration)
        {
            var wordSearchesFile = Path.Combine("Data", "wordSearches.json");
            
            //WordSearches = ParseScrapedWordSearchResponse();
            //File.WriteAllText(wordSearchesFile, JsonSerializer.Serialize(WordSearches, new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //}));
            //CheckArchivedWordSearches(configuration);
            
            // todo: anything at level 0 is ignored by the game
            // it's all campaign stuff
            // the order is also sorted by level on the server side
            WordSearches = JsonSerializer.Deserialize<PuzzleDefinition[]>(File.ReadAllText(wordSearchesFile))!;
        }

        private void CheckArchivedWordSearches(IConfiguration configuration)
        {
            var archivePathA = Path.Combine(configuration["ArchivePath"]!, "externalUIs", "wordSearch");
            var archivePathB = Path.Combine(configuration["ArchivePath"]!, "play", "externalUIs", "wordSearch");

            var okayCount = 0;
            var missingCount = 0;
            foreach (var puzzle in WordSearches)
            {
                if (!Path.Exists(Path.Combine(archivePathA, puzzle.m_configPath)) &&
                    !Path.Exists(Path.Combine(archivePathB, puzzle.m_configPath)))
                {
                    Console.Out.WriteLine($"{puzzle.m_configPath} (\"{puzzle.m_name}\") wasn't archived");
                    missingCount++;
                } else
                {
                    okayCount++;
                }
            }
            
            Console.Out.WriteLine($"{okayCount} archived");
            Console.Out.WriteLine($"{missingCount} missing");
        }

        private static PuzzleDefinition[] ParseScrapedWordSearchResponse()
        {
            var dumpedData = File.ReadAllText(@"E:\re\bw\archive\lb.binweevils.com\php\getPuzzleList.php");
            var options = new FormOptions();
            var puzzleList = options.Deserialize<GetPuzzleListResponse>(dumpedData);

            var puzzleFiles = puzzleList.m_configList.Split('|');
            var puzzleNames = puzzleList.m_nameList.Split('|');
            var puzzleLevels = puzzleList.m_levelList.Split('|');
            Debug.Assert(puzzleFiles.Length == puzzleNames.Length);
            Debug.Assert(puzzleFiles.Length == puzzleLevels.Length);
            
            var puzzles = new PuzzleDefinition[puzzleFiles.Length];
            for (var i = 0; i < puzzleFiles.Length; i++)
            {
                puzzles[i] = new PuzzleDefinition
                {
                    m_configPath = puzzleFiles[i],
                    m_name = puzzleNames[i],
                    m_level = byte.Parse(puzzleLevels[i])
                };
            }

            return puzzles;
        }
    }

    public class PuzzleDefinition
    {
        [JsonPropertyName("configPath")] public string m_configPath { get; set; }
        [JsonPropertyName("name")] public string m_name { get; set; }
        [JsonPropertyName("level")] public byte m_level { get; set; }
    }
}