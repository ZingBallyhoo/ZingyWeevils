using BinWeevils.Protocol.Xml.Puzzle;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class PuzzleConfigRepositoryOptions<TPuzzle>
    {
        public string ConfigPath { get; set; } = "";
    }
    
    public interface IPuzzleConfigRepository
    {
        string ConfigPath { get; }
        IReadOnlyDictionary<int, PuzzleDefinition> Puzzles { get; }
    }
    
    public class PuzzleConfigRepository<TPuzzle> : IPuzzleConfigRepository
        where TPuzzle : PuzzleBase, new()
    {
        private readonly ILogger<PuzzleConfigRepository<TPuzzle>> m_logger;
        private readonly IFileProvider m_fileProvider;
        private readonly IOptionsMonitor<PuzzleConfigRepositoryOptions<TPuzzle>> m_optionsMonitor;
        
        private readonly Dictionary<int, PuzzleDefinition> m_puzzles = [];
        private readonly Dictionary<int, TPuzzle> m_puzzleConfigs = [];

        public string ConfigPath => m_optionsMonitor.CurrentValue.ConfigPath;
        public IReadOnlyDictionary<int, PuzzleDefinition> Puzzles => m_puzzles;
        public IReadOnlyDictionary<int, TPuzzle> PuzzleConfigs => m_puzzleConfigs;
        
        public PuzzleConfigRepository(
            ILogger<PuzzleConfigRepository<TPuzzle>> logger,
            IFileProvider fileProvider,
            IOptionsMonitor<PuzzleConfigRepositoryOptions<TPuzzle>> optionsMonitor)
        {
            m_logger = logger;
            m_fileProvider = fileProvider;
            m_optionsMonitor = optionsMonitor;
        }

        public void AddPuzzle(PuzzleDefinition puzzleDefinition)
        {
            var fileInfo = m_fileProvider.GetFileInfo(Path.Combine(ConfigPath, puzzleDefinition.m_configPath));
            if (!fileInfo.Exists)
            {
                var severity = puzzleDefinition.m_level switch
                {
                    0 => LogLevel.Trace, // campaign, doesn't matter
                    _ => LogLevel.Critical
                };
                m_logger.Log(severity,"Puzzle {ConfigPath} doesn't exist on disk", puzzleDefinition.m_configPath);
                return;
            }
            
            var stringContents = fileInfo.GetStringContents();

            TPuzzle puzzleConfig;
            try
            {
                puzzleConfig = XmlReadBuffer.ReadStatic<TPuzzle>(stringContents, CDataMode.Off);
            } catch (Exception e)
            {
                m_logger.LogCritical(e, "Unable to parse puzzle {FileName}", fileInfo.Name);
                return;
            }

            if (m_puzzles.TryGetValue(puzzleConfig.m_id, out var existingDefinition))
            {
                m_logger.LogError("Puzzle {ID} has ID conflict: \"{Def1}\" & \"{Def2}\"", puzzleConfig.m_id, existingDefinition.m_name, puzzleDefinition.m_name);
                return;
            }
            
            m_puzzles.Add(puzzleConfig.m_id, puzzleDefinition);
            m_puzzleConfigs.Add(puzzleConfig.m_id, puzzleConfig);
        }
        
        public void AddPuzzles(IEnumerable<PuzzleDefinition> enumerable)
        {
            foreach (var puzzle in enumerable)
            {
                AddPuzzle(puzzle);
            }
        }
    }
}