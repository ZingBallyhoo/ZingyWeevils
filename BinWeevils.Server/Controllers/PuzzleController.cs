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
        private readonly PuzzleRepository m_repository;

        public PuzzleController(PuzzleRepository repository)
        {
            m_repository = repository;
        }
        
        [HttpPost("php/getPuzzleList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public GetPuzzleListResponse GetPuzzleList()
        {
            // params: userID, typeID
            
            // 1 = ws
            // 2 = cw
            
            IPuzzleConfigRepository configRepo;
            string typeName;
            string gamePath;
            string locName;
            switch (1)
            {
                case 1:
                {
                    configRepo = m_repository.WordSearches;
                    typeName = "wordsearch";
                    gamePath = "externalUIs/wordSearch_11_02_11.swf";
                    locName = "doing a wordsearch";
                    break;
                }
                case 2:
                {
                    configRepo = m_repository.Crosswords;
                    typeName = "crossword";
                    gamePath = "externalUIs/crossword2.swf";
                    locName = "doing a crossword";
                    break;
                }
                default:
                {
                    throw new InvalidDataException("invalid puzzle type");
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
        
        // todo:
        // php/saveCrosswordProgress.php
        // gridID	"91"
        // completed	"1"
        // progress	".....S...........PILOT....S.W.C..R...AUTUMN.H.A.BARK.M....O.E.N..N...P..C.W.E.D..K......O.F.L.L.......SUNGLASSES......A.E.A...S...SEVEN...K........A..D...E........S..W...S........T..I.S.............CROWN...........H.C...............K...............S......."
        // userID	"zingy"
    }
}