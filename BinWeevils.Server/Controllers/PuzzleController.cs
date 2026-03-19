using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Form.Puzzle;
using BinWeevils.Protocol.Xml.Puzzle;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackXML.Str;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class PuzzleController : Controller
    {
        private readonly PuzzleConfigRepository<WordSearch> m_wordSearches;
        private readonly PuzzleConfigRepository<Crossword> m_crosswords;
        private readonly WeevilDBContext m_dbContext;

        public PuzzleController(
            PuzzleConfigRepository<WordSearch> wordSearches,
            PuzzleConfigRepository<Crossword> crosswords,
            WeevilDBContext dbContext)
        {
            m_wordSearches = wordSearches;
            m_crosswords = crosswords;
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("php/getPuzzleList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetPuzzleListResponse> GetPuzzleList([FromBody] GetPuzzleListRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PuzzleController.GetPuzzleList");
            activity?.SetTag("userID", request.m_userID);
            activity?.SetTag("typeID", request.m_typeID);
            
            if (request.m_userID != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to get someone else's puzzle list");
            }
            
            IPuzzleConfigRepository configRepo;
            string typeName;
            string gamePath;
            string locName;
            HashSet<byte> completedPuzzles;
            
            switch (request.m_typeID)
            {
                case PuzzleTypeID.WordSearch:
                {
                    configRepo = m_wordSearches;
                    typeName = "wordsearch";
                    gamePath = "externalUIs/wordSearch_11_02_11.swf";
                    locName = "doing a wordsearch";
                    completedPuzzles = await m_dbContext.m_wordSearchProgress
                        .Where(x => x.m_weevil.m_name == request.m_userID)
                        .Where(x => x.m_complete)
                        .Select(x => x.m_puzzleID)
                        .ToHashSetAsync();
                    break;
                }
                case PuzzleTypeID.Crossword:
                {
                    configRepo = m_crosswords;
                    typeName = "crossword";
                    gamePath = "externalUIs/crossword2.swf";
                    locName = "doing a crossword";
                    completedPuzzles = await m_dbContext.m_crosswordProgress
                        .Where(x => x.m_weevil.m_name == request.m_userID)
                        .Where(x => x.m_complete)
                        .Select(x => x.m_puzzleID)
                        .ToHashSetAsync();
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"invalid puzzle type: {request.m_typeID}");
                }
            }

            var puzzleList = configRepo.Puzzles.Values.ToArray();
            var completedList = configRepo.Puzzles.Keys
                .Select(x => completedPuzzles.Contains(checked((byte)x)))
                .Select(x => x ? '1' : '0');
            
            return new GetPuzzleListResponse
            {
                m_typeName = typeName,
                m_gamePath = gamePath,
                m_configBasePath = $"{configRepo.ConfigPath}/",
                m_locName = locName,
                m_levelList = string.Join('|', puzzleList.Select(x => x.Level.ToString())),
                m_nameList = string.Join('|', puzzleList.Select(x => x.Name.ToString())),
                m_configList = string.Join('|', puzzleList.Select(x => x.ConfigPath.ToString())),
                m_completedList = string.Join('|', completedList),
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

            if (!m_wordSearches.PuzzleConfigs.TryGetValue(request.m_gridID, out var wordSearch))
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
            if (!m_crosswords.PuzzleConfigs.TryGetValue(request.m_gridID, out var crossWord))
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