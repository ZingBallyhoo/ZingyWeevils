using System.Diagnostics;
using System.Text.Json.Serialization;
using ArcticFox.PolyType.FormEncoded;
using BinWeevils.Protocol.Form.Puzzle;

namespace BinWeevils.Server.Services
{
    public class PuzzleDefinition
    {
        [JsonPropertyName("configPath")] public string ConfigPath { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("level")] public byte Level { get; set; }

        public PuzzleDefinition()
        {
        }

        public PuzzleDefinition(string configPath, string name, byte level)
        {
            ConfigPath = configPath;
            Name = name;
            Level = level;
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
                    ConfigPath = puzzleFiles[i],
                    Name = puzzleNames[i],
                    Level = byte.Parse(puzzleLevels[i])
                };
            }
            
            //File.WriteAllText(wordSearchesFile, JsonSerializer.Serialize(wordSearches, new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //}));

            return puzzles;
        }
    }
}