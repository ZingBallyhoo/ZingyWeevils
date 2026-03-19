using BinWeevils.Protocol.Xml.Puzzle;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class PuzzlesOptionsPostConfigure<TPuzzle> : IPostConfigureOptions<PuzzlesOptions<TPuzzle>> where TPuzzle : PuzzleBase, new()
    {
        private readonly ILogger<PuzzlesOptionsPostConfigure<TPuzzle>> m_logger;
        private readonly IFileProvider m_fileProvider;
        
        public PuzzlesOptionsPostConfigure(
            ILogger<PuzzlesOptionsPostConfigure<TPuzzle>> logger,
            IFileProvider fileProvider)
        {
            m_logger = logger;
            m_fileProvider = fileProvider;
        }
        
        public void PostConfigure(string? name, PuzzlesOptions<TPuzzle> options)
        {
            options.Puzzles.Clear();
            options.PuzzleConfigs.Clear();

            foreach (var puzzleDefinition in options.RawPuzzles)
            {
                AddPuzzle(options, puzzleDefinition);
            }
        }
        
        private void AddPuzzle(PuzzlesOptions<TPuzzle> options, PuzzleDefinition puzzleDefinition)
        {
            var fileInfo = m_fileProvider.GetFileInfo(Path.Combine(options.ConfigPath, puzzleDefinition.ConfigPath));
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

            if (options.Puzzles.TryGetValue(puzzleConfig.m_id, out var existingDefinition))
            {
                m_logger.LogError("Puzzle {ID} has ID conflict: \"{Def1}\" & \"{Def2}\"", puzzleConfig.m_id, existingDefinition.Name, puzzleDefinition.Name);
                return;
            }
            
            options.Puzzles.Add(puzzleConfig.m_id, puzzleDefinition);
            options.PuzzleConfigs.Add(puzzleConfig.m_id, puzzleConfig);
        }
    }
}