using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Form.Puzzle;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackXML.Str;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class PuzzleController : Controller
    {
        private readonly IOptionsMonitor<WordSearchesOptions> m_wordSearches;
        private readonly IOptionsMonitor<CrosswordsOptions> m_crosswords;
        private readonly WeevilDBContext m_dbContext;

        public PuzzleController(
            IOptionsMonitor<WordSearchesOptions> wordSearches,
            IOptionsMonitor<CrosswordsOptions> crosswords,
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
            
            PuzzlesOptions puzzlesOptions;
            string typeName;
            string gamePath;
            string locName;
            IQueryable<WeevilPuzzleProgressDB> puzzleProgressQueryable;
            
            switch (request.m_typeID)
            {
                case PuzzleTypeID.WordSearch:
                {
                    puzzlesOptions = m_wordSearches.CurrentValue;
                    typeName = "wordsearch";
                    gamePath = "externalUIs/wordSearch_11_02_11.swf";
                    locName = "doing a wordsearch";
                    puzzleProgressQueryable = m_dbContext.m_wordSearchProgress;
                    break;
                }
                case PuzzleTypeID.Crossword:
                {
                    puzzlesOptions = m_crosswords.CurrentValue;
                    typeName = "crossword";
                    gamePath = "externalUIs/crossword2.swf";
                    locName = "doing a crossword";
                    puzzleProgressQueryable = m_dbContext.m_crosswordProgress;
                    break;
                }
                default:
                {
                    throw new InvalidDataException($"invalid puzzle type: {request.m_typeID}");
                }
            }
            
            var allCompletedPuzzles = await puzzleProgressQueryable
                .Where(x => x.m_weevil.m_name == request.m_userID)
                .Where(x => x.m_complete)
                .Select(x => x.m_puzzleID)
                .ToHashSetAsync();
            var completedList = puzzlesOptions.Puzzles.Keys
                .Select(x => allCompletedPuzzles.Contains(checked((byte)x)))
                .Select(x => x ? '1' : '0');

            var puzzleList = puzzlesOptions.Puzzles.Values;
            return new GetPuzzleListResponse
            {
                m_typeName = typeName,
                m_gamePath = gamePath,
                m_configBasePath = $"{puzzlesOptions.ConfigPath}/",
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
            // todo: user id
            
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
            
            // todo: user id

            var wordSearches = m_wordSearches.CurrentValue;
            if (!wordSearches.PuzzleConfigs.TryGetValue(request.m_gridID, out var wordSearch))
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

        [StructuredFormPost("php/getCrosswordProgress.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetCrosswordProgressResponse> GetCrosswordProgress([FromBody] GetPuzzleProgressRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PuzzleController.GetCrosswordProgress");
            activity?.SetTag("userID", request.m_userID);
            activity?.SetTag("gridID", request.m_gridID);
            
            if (request.m_userID != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to get someone else's crossword progress");
            }

            var crosswords = m_crosswords.CurrentValue;
            if (!crosswords.PuzzleConfigs.TryGetValue(request.m_gridID, out var crossword))
            {
                throw new InvalidDataException("trying to get progress of a crossword that doesn't exist");
            }

            var userProgress = await m_dbContext.m_crosswordProgress
                .Where(x => x.m_weevil.m_name == request.m_userID)
                .SingleOrDefaultAsync(x => x.m_puzzleID == request.m_gridID);

            return new GetCrosswordProgressResponse
            {
                m_progress = userProgress?.m_progress ?? "0",
                m_completed = userProgress?.m_complete ?? false
            };
        }
        
        [StructuredFormPost("php/saveCrosswordProgress.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<SaveCrosswordProgressResponse> SaveCrosswordProgress([FromBody] SaveCrosswordProgressRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("PuzzleController.SaveCrosswordProgress");
            activity?.SetTag("userID", request.m_userID);
            activity?.SetTag("gridID", request.m_gridID);
            activity?.SetTag("progress", request.m_progress);
            activity?.SetTag("completed", request.m_completed);
            
            if (request.m_userID != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new Exception("trying to save someone else's crossword progress");
            }

            var crosswords = m_crosswords.CurrentValue;
            if (!crosswords.PuzzleConfigs.TryGetValue(request.m_gridID, out var crossword))
            {
                throw new InvalidDataException("trying to save a crossword that doesn't exist");
            }

            var completedText = crossword.GetSolutionText();
            if (request.m_progress.Length != completedText.Length)
            {
                throw new InvalidDataException($"wrong crossword solution length. got {request.m_progress.Length}, expected {completedText.Length}");
            }

            for (var i = 0; i < completedText.Length; i++)
            {
                if (char.IsAsciiLetterUpper(request.m_progress[i]))
                {
                    if (completedText[i] == '.') throw new InvalidDataException("attempt to save into blank space of crossword solution");
                } else if (request.m_progress[i] != '.') 
                {
                    throw new InvalidDataException($"invalid char \"{request.m_progress[i]}\" in crossword solution");
                }
            }

            // note: the player can save the completed result without checking the answers, and therefore have m_completed = false
            var actuallyCompleted = completedText.Equals(request.m_progress, StringComparison.InvariantCultureIgnoreCase);
            if (request.m_completed && !actuallyCompleted)
            {
                throw new InvalidDataException("completed status of request doesn't match expected");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();

            var idx = await m_dbContext.GetIdx(request.m_userID);
            var rowsUpdated = await m_dbContext.m_crosswordProgress
                .Upsert(new WeevilCrosswordProgressDB
                {
                    m_weevilIdx = idx,
                    m_puzzleID = request.m_gridID,
                    m_complete = request.m_completed,
    
                    m_progress = request.m_progress,
                })
                .UpdateIf(x => x.m_complete == false)
                .RunAsync();
            
            if (rowsUpdated != 1)
            {
                await transaction.RollbackAsync();
                
                // todo: what result code?
                return new SaveCrosswordProgressResponse();
            }
            
            SaveCrosswordProgressResponse response;
            if (request.m_completed)
            {
                // todo: xp from options
                await m_dbContext.GiveMulchAndXp(idx, crossword.m_reward, crosswords.XpReward);
                var mulchAndXp = await m_dbContext.GetMulchAndXp(idx);
                
                // todo: metrics

                response = new SaveCrosswordProgressResponse
                {
                    m_result = SaveCrosswordProgressResponse.RESULT_COMPLETED,
                    m_mulch = mulchAndXp.m_mulch,
                    m_xp = mulchAndXp.m_xp,
                };
            } else
            {
                // todo: what result code?
                response = new SaveCrosswordProgressResponse();
            }
            await transaction.CommitAsync();
            
            return response;
        }
    }
}