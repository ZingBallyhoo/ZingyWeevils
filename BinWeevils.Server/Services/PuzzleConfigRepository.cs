using BinWeevils.Protocol.Xml.Puzzle;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class PuzzleConfigRepositoryOptions<TPuzzle>
    {
        public string ConfigPath { get; set; } = "";
        public List<PuzzleDefinition> Puzzles { get; set; } = [];
    }
    
    public interface IPuzzleConfigRepository
    {
        string ConfigPath { get; }
        IReadOnlyDictionary<int, PuzzleDefinition> Puzzles { get; }
    }
    
    public class PuzzleConfigRepository<TPuzzle> : IPuzzleConfigRepository, IDisposable
        where TPuzzle : PuzzleBase, new()
    {
        private readonly ILogger<PuzzleConfigRepository<TPuzzle>> m_logger;
        private readonly IFileProvider m_fileProvider;
        private readonly IOptionsMonitor<PuzzleConfigRepositoryOptions<TPuzzle>> m_optionsMonitor;
        private readonly IDisposable? m_onChangeListener;
        
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

            ReloadPuzzles();
            m_onChangeListener = optionsMonitor.OnChange(_ =>
            {
                ReloadPuzzles();
            });
        }

        private void ReloadPuzzles()
        {
            // todo: yes, this can race after we clear and before we add back
            // but this isn't supposed to be reloading in prod
            
            m_puzzles.Clear();
            m_puzzleConfigs.Clear();

            foreach (var puzzleDefinition in m_optionsMonitor.CurrentValue.Puzzles)
            {
                AddPuzzle(puzzleDefinition);
            }
        }

        private void AddPuzzle(PuzzleDefinition puzzleDefinition)
        {
            var fileInfo = m_fileProvider.GetFileInfo(Path.Combine(ConfigPath, puzzleDefinition.ConfigPath));
            if (!fileInfo.Exists)
            {
                var severity = puzzleDefinition.Level switch
                {
                    0 => LogLevel.Trace, // campaign, doesn't matter
                    _ => LogLevel.Critical
                };
                m_logger.Log(severity,"Puzzle {ConfigPath} doesn't exist on disk", puzzleDefinition.ConfigPath);
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
                m_logger.LogError("Puzzle {ID} has ID conflict: \"{Def1}\" & \"{Def2}\"", puzzleConfig.m_id, existingDefinition.Name, puzzleDefinition.Name);
                return;
            }
            
            m_puzzles.Add(puzzleConfig.m_id, puzzleDefinition);
            m_puzzleConfigs.Add(puzzleConfig.m_id, puzzleConfig);
        }

        public void Dispose()
        {
            m_onChangeListener?.Dispose();
        }
    }
}