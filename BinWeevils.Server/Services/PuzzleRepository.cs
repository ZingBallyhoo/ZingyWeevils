using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcticFox.PolyType.FormEncoded;
using BinWeevils.Protocol.Form;
using Microsoft.Extensions.FileProviders;

namespace BinWeevils.Server.Services
{
    public class PuzzleRepository
    {
        public PuzzleDefinition[] WordSearches { get; }
        public PuzzleDefinition[] Crosswords { get; }
        
        public PuzzleRepository(IConfiguration configuration, IFileProvider fileProvider)
        {
            var wordSearchesFile = Path.Combine("Data", "wordSearches.json");
            
            //WordSearches = ParseScrapedWordSearchResponse();
            //File.WriteAllText(wordSearchesFile, JsonSerializer.Serialize(WordSearches, new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //}));
            //CheckArchivedWordSearches(fileProvider);
            
            // todo: anything at level 0 is ignored by the game
            // it's all campaign stuff
            // the order is also sorted by level on the server side
            WordSearches = JsonSerializer.Deserialize<PuzzleDefinition[]>(File.ReadAllText(wordSearchesFile))!;

            // no scraped data, so list is reconstructed
            // https://binweevilcompany.wordpress.com/puzzles/crossword-answers/
            // list names from https://www.youtube.com/watch?v=CBzBl5vWMLw
            Crosswords = [
                new PuzzleDefinition("xmas2015.xml", "Christmas 2015", 1),
                new PuzzleDefinition("crossword91.xml", "General Crossword", 1),
                new PuzzleDefinition("crossword92.xml", "Opposites", 2),
                new PuzzleDefinition("crossword99.xml", "The Great Outdoors", 3),
                new PuzzleDefinition("crossword96.xml", "Jobs and Work", 4),
                new PuzzleDefinition("crossword95.xml", "General Crossword", 5),
                new PuzzleDefinition("crossword93.xml", "Music Mania", 6),
                new PuzzleDefinition("crossword94.xml", "General Crossword", 7),
                new PuzzleDefinition("crossword97.xml", "Pets", 8),
                new PuzzleDefinition("crossword98_1.xml", "Around the Binscape", 9),
                new PuzzleDefinition("crossword100.xml", "Science Rocks", 10),
                new PuzzleDefinition("crossword101.xml", "Summer Fun", 12),
            ];
        }

        private void CheckArchivedWordSearches(IFileProvider fileProvider)
        {
            var okayCount = 0;
            var missingCount = 0;
            foreach (var puzzle in WordSearches)
            {
                var fileInfo = fileProvider.GetFileInfo(Path.Combine("externalUIs", "wordSearch", puzzle.m_configPath));
                if (!fileInfo.Exists)
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

        public PuzzleDefinition()
        {
        }

        public PuzzleDefinition(string configPath, string name, byte level)
        {
            m_configPath = configPath;
            m_name = name;
            m_level = level;
        }
    }
}