using BinWeevils.Protocol.Xml.Puzzle;

namespace BinWeevils.Server.Services
{
    public class PuzzlesOptions
    {
        public string ConfigPath { get; set; } = "";
        public List<PuzzleDefinition> RawPuzzles { get; set; } = [];
        
        public OrderedDictionary<int, PuzzleDefinition> Puzzles { get; set; } = [];
    }
    
    public class PuzzlesOptions<TPuzzle> : PuzzlesOptions
    {
        public OrderedDictionary<int, TPuzzle> PuzzleConfigs { get; set; } = [];
    }

    public class WordSearchesOptions : PuzzlesOptions<WordSearch>
    {
        
    }

    public class CrosswordsOptions : PuzzlesOptions<Crossword>
    {
        public uint XpReward { get; set; } = 30;
    }
}