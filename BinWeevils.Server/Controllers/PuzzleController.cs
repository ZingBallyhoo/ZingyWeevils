using System.Net.Mime;
using BinWeevils.Protocol.Form.Puzzle;
using BinWeevils.Protocol.Xml.Puzzle;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackXML.Str;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class PuzzleController : Controller
    {
        private readonly PuzzleRepository m_repository;

        public PuzzleController(PuzzleRepository repository)
        {
            m_repository = repository;
        }
        
        [StructuredFormPost("php/getPuzzleList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public GetPuzzleListResponse GetPuzzleList([FromBody] GetPuzzleListRequest request)
        {
            IPuzzleConfigRepository configRepo;
            string typeName;
            string gamePath;
            string locName;
            switch (request.m_typeID)
            {
                case PuzzleTypeID.WordSearch:
                {
                    configRepo = m_repository.WordSearches;
                    typeName = "wordsearch";
                    gamePath = "externalUIs/wordSearch_11_02_11.swf";
                    locName = "doing a wordsearch";
                    break;
                }
                case PuzzleTypeID.Crossword:
                {
                    configRepo = m_repository.Crosswords;
                    typeName = "crossword";
                    gamePath = "externalUIs/crossword2.swf";
                    locName = "doing a crossword";
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"invalid puzzle type: {request.m_typeID}");
                }
            }

            var puzzleList = configRepo.Puzzles.Values.ToArray();
            return new GetPuzzleListResponse
            {
                m_typeName = typeName,
                m_gamePath = gamePath,
                m_configBasePath = $"{configRepo.ConfigPath}/",
                m_locName = locName,
                m_levelList = string.Join('|', puzzleList.Select(x => x.m_level.ToString())),
                m_nameList = string.Join('|', puzzleList.Select(x => x.m_name.ToString())),
                m_configList = string.Join('|', puzzleList.Select(x => x.m_configPath.ToString())),
                m_completedList = string.Join('|', puzzleList.Select(x => '0')),
            };
        }
        
        [HttpPost("php/getWordSearchProgress.php")]
        public string GetWordSearchProgress()
        {
            return "result=0";
            return "result=0,11,4,11";
            return "result=0,11,4,11|13,13,13,6";
        }
        
        [StructuredFormPost("php/saveWordSearchProgress.php")]
        public string SaveWordSearchProgress([FromBody] SaveWordSearchProgressRequest request)
        {
            // gridID "24"
            // completed "0"
            // progress	"1,15,6,15"
            // userID "zingy"

            if (!m_repository.WordSearches.PuzzleConfigs.TryGetValue(request.m_gridID, out var wordSearch))
            {
                throw new InvalidDataException("trying to solve a word search that doesn't exist");
            }

            // the latest span will always be appended at the end of the progress string
            // if this is a lie, we will figure out later
            var newestSpan = request.m_progress.m_spans.Last();

            var spanText = wordSearch.ReadSpan(newestSpan);
            if (!wordSearch.IsWord(spanText))
            {
                throw new InvalidDataException($"{spanText} ({newestSpan.AsString(',')}) is not a word in {request.m_gridID}:\"{wordSearch.m_heading}\"");
            }

            return "";
        }
        
        [StructuredFormPost("php/saveCrosswordProgress.php")]
        public string SaveCrosswordProgress([FromBody] SaveCrosswordProgressRequest request)
        {
            if (!m_repository.Crosswords.PuzzleConfigs.TryGetValue(request.m_gridID, out var crossWord))
            {
                throw new InvalidDataException("trying to solve a crossword that doesn't exist");
            }

            var completedText = crossWord.GetSolutionText();
            if (request.m_progress.Length != completedText.Length)
            {
                throw new InvalidDataException($"wrong crossword solution length. got {request.m_progress.Length}, expected {completedText.Length}");
            }

            for (var i = 0; i < completedText.Length; i++)
            {
                if (char.IsAsciiLetterUpper(request.m_progress[i]))
                {
                    if (completedText[i] == '-') throw new InvalidDataException("attempt to save into blank space of crossword solution");
                } else if (request.m_progress[i] != '.') 
                {
                    throw new InvalidDataException($"invalid char \"{request.m_progress[i]}\" in crossword solution");
                }
            }

            var actuallyCompleted = completedText.Equals(request.m_progress, StringComparison.InvariantCultureIgnoreCase);
            if (actuallyCompleted != request.m_completed)
            {
                throw new InvalidDataException("completed status of request doesn't match expected");
            }
            
            return "";
        }
    }
}