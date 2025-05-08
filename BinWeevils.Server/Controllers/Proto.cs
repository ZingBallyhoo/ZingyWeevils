using System.Net.Mime;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("")]
    public class Proto : Controller
    {
        [HttpGet("api/site/zones")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public ActiveZonesResponse GetActiveZones()
        {
            return new ActiveZonesResponse
            {
                m_names = ["Grime"],
                m_ips = ["127.0.0.1"],
                m_outOf5 = [5],
                m_responseCode = true
            };
        }
        
        [HttpGet("api/php/getTreasureHunt.php")]
        public string GetTreasureHunt()
        {
            return "responseCode=2&failed=0";
        }
        
        [HttpGet("api/php/getMyLottoTicketsAndDrawDate.php")]
        public string GetMyLottoTickets()
        {
            return "responseCode=1&nextDraw=2020-08-18+17%3A00%3A00&drawID=1479&gotTicket=0&tickets=0";
        }
        
        [HttpPost("api/weevil/geo")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public GeoResponse GetGeo()
        {
            return new GeoResponse
            {
                m_l = "uk"
            };
        }
        
        [HttpGet("api/site/server-time")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public ServerTimeResponse GetServerTime([FromServices] TimeProvider timeProvider)
        {
            return new ServerTimeResponse
            {
                m_result = true,
                m_time = timeProvider.GetUtcNow().ToUnixTimeSeconds()
            };
        }
        
        [HttpPost("api/php/hardestTrackUnlocked.php")]
        public string HardestUnlockedTrack()
        {
            // i dont really want to implement this even though tracks are "unlocked"
            return "res=1";
        }
        
        [HttpPost("api/php/isTrackUnlocked.php")]
        public string IsTrackUnlocked()
        {
            return "res=1";
        }
        
        // fp uses GET, ruffle uses POST
        // todo: (the code intends to use POST but fp behaviour falls back to GET when no body)
        [HttpPost("api/php/getMissionList.php")]
        [HttpGet("api/php/getMissionList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public MissionList GetMissionList()
        {
            return new MissionList
            {
                m_responseCode = 1,
                m_ids = "1|2|3|4|5|6|7|8|9|10|11|12|13|14|16|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|37|37|38|39|40|41|43|45|46|47|48|49|50|51|52|53|54|55|56|57|58|59|60|61|62|63|64|65|67|68|69|72|73|74|75|76|77", 
                m_names = "Join the SWS|The Lost Silver Knight|The Blue Diamond (Part 1)|The Blue Diamond (Part 2)|Totem of the Aztecs|Jack and the Binstalk|Trouble at Castle Gam (Part 1)|Trouble at Castle Gam (Part 2)|Raiders of the Lost Bin Pet|The Hunt for Weevil X|Gun Training|Danger at Dosh's Palace|Showdown at Tycoon TV Towers|Laboratory Lockdown|New Users Tasks|New Users Tasks|Case File 1: Good vs WeEvil|Case File 2: Micro Mayhem|WeeklyChallenge1 Part1 - Missing Golden Bin Pet Ball|WeeklyChallenge1 Part2 - Missing Golden Bin Pet Ball|WeeklyChallenge1 Part3 - Missing Golden Bin Pet Ball|WeeklyChallenge2 Part1|WeeklyChallenge2 Part2|WeeklyChallenge2 Part3|WeeklyChallenge3 Part1|WeeklyChallenge3 Part2|WeeklyChallenge3 Part3|WeeklyChallenge4 Part1|WeeklyChallenge4 Part2|WeeklyChallenge4 Part3|WeeklyChallenge5 Part1|WeeklyChallenge5 Part2|WeeklyChallenge5 Part3|WeeklyChallenge6 Part1|WeeklyChallenge6 Part2|WeeklyChallenge6 Part3|Bin Bot Potions|Bin Bot Potions|Bin Bot Potions|WeeklyChallenge7 Part1|WeeklyChallenge7 Part2|WeeklyChallenge7 Part3|Super Antenna Hunt|Case File 3: Scribbles the secret hunter|Halloween 2013 treasure room|Bin Pets VIP gold gift set|Disco Tycoon rewards|Easter Hunt|Random Tasks|Summer Fair Fun House|Halloween 2014|The Bin's Big Freeze|Advent Calendar 2014|Snow Weevil Hunt 2014|SWS vs WEB hunt|Halloween Pumpkin Hunt 2015|Airfix2015|MLP Nest Bundle 1|MLP Nest Bundle 2|Minions Nest Bundle|The Good Dino Nest Bundle|Transformers 2016 Statue pack|Transformers 2016 Poster pack|LEGO Friends Adventure Club Nest Items|Plants Vs Zombies Garden Warfare 2 Nest Items|Zootropolis nest items|Big Bloom|History Hunters|MLPS6 Stamp Card|Transformers2017|Transformers S2 Ultimate Fan|Weevil World Tutorial|Hot Wheels Weevil World Hunt 2018|Weevil World Pumpkin Hunt", 
                m_paths = "|silverKnight|blueDiamond1|blueDiamond2|Aztec|giant|CastleQuest_1|CastleQuest_2|lostBinPet/intro|gemHunt/intro||doshHeritage/intro|tycoonTerrorTowers/intro|laboratoryLockDown/intro||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||", 
                levelList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                completedList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                tycoonList = "1|1|1|1|1|1|1|1|0|1|1|1|1|1|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1|1|0|1|1|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                roomList = "2|0|0|0|0|0|0|0|1|1|2|1|1|1|2|2|3|3|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|3|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|0|0|0", 
                priceList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreBronze = "0|0|0|0|0|0|0|0|6|6|0|6|5|7|0|0|40|14|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|12|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreSilver = "0|0|0|0|0|0|0|0|12|12|0|12|10|14|0|0|50|21|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|18|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreGold = "0|0|0|0|0|0|0|0|18|18|0|18|15|24|0|0|60|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scorePlatinum = "0|0|0|0|0|0|0|0|24|24|0|24|18|34|0|0|70|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                highScore = "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||"
            };
        }
        
        [HttpPost("api/weevil/update-user-info")]
        public Task UpdateUserInfo()
        {
            // form: idx
            
            // todo: what is this for?
            // stubbed just to prevent 404s
            return Task.CompletedTask;
        }
        
        [HttpPost("tycoon/startSession.php")]
        public Task BeginTycoonSession()
        {
            // form: bunch of stuff
            
            // todo: stubbed just to prevent 404s
            return Task.CompletedTask;
        }
    }
}