using BinWeevils.Protocol.Xml;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class PuzzleConfigRepositoryOptions<TPuzzle>
    {
        public string ConfigPath { get; set; } = "";
    }
    
    public class PuzzleConfigRepository<TPuzzle> where TPuzzle : PuzzleBase, IXmlSerializable, new()
    {
        public IReadOnlyDictionary<int, TPuzzle> Puzzles { get; }
        
        public PuzzleConfigRepository(
            ILogger<PuzzleConfigRepository<TPuzzle>> logger,
            IFileProvider fileProvider,
            IOptionsMonitor<PuzzleConfigRepositoryOptions<TPuzzle>> optionsMonitor)
        {
            var puzzles = new Dictionary<int, TPuzzle>();
            foreach (var fileInfo in fileProvider.GetDirectoryContents(optionsMonitor.CurrentValue.ConfigPath))
            {
                var stringContents = fileInfo.GetStringContents();

                TPuzzle puzzleConfig;
                try
                {
                    puzzleConfig = XmlReadBuffer.ReadStatic<TPuzzle>(stringContents, CDataMode.Off);
                } catch (Exception e)
                {
                    logger.LogCritical(e, "Unable to parse puzzle {FileName}", fileInfo.Name);
                    continue;
                }

                puzzles.Add(puzzleConfig.m_id, puzzleConfig);
            }

            Puzzles = puzzles;
        }
    }
}