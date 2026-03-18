using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}