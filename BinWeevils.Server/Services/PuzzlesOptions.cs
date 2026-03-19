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
        public uint MulchRewardPerWord { get; set; } = 10;
        public uint MulchRewardComplete { get; set; } = 100;
        public uint XpRewardComplete { get; set; } = 10;
    }

    public class CrosswordsOptions : PuzzlesOptions<Crossword>
    {
        public uint XpReward { get; set; } = 30;
    }
}