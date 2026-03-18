using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
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
        [HttpPost("php/getPuzzleList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public GetPuzzleListResponse GetPuzzleList([FromServices] PuzzleRepository repo)
        {
            // params: userID, typeID

            return new GetPuzzleListResponse
            {
                m_typeName = "wordsearch",
                m_gamePath = "externalUIs/wordSearch_11_02_11.swf",
                m_configBasePath = "externalUIs/wordSearch/",
                m_locName = "doing a wordsearch",
                m_levelList = string.Join('|', repo.WordSearches.Select(x => x.m_level.ToString())),
                m_nameList = string.Join('|', repo.WordSearches.Select(x => x.m_name.ToString())),
                m_configList = string.Join('|', repo.WordSearches.Select(x => x.m_configPath.ToString())),
                m_completedList = string.Join('|', repo.WordSearches.Select(x => '0')),
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
        public string SaveWordSearchProgress([FromBody] SaveWordSearchProgressRequest request, [FromServices] PuzzleConfigRepository<WordSearch> wordSearchConfigs)
        {
            // gridID "24"
            // completed "0"
            // progress	"1,15,6,15"
            // userID "zingy"

            if (!wordSearchConfigs.Puzzles.TryGetValue(request.m_gridID, out var wordSearch))
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
    }
}