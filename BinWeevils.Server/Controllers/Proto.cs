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
        
        [HttpPost("api/pets/defs")]
        public string GetPetDefs()
        {
            return "result=";
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
        public string GetServerTime()
        {
            return "res=1&t=1597760479&x=y";
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
        
        [HttpPost("api/php/getPlaylistIDsForRoom.php")]
        public string GetPlayListIdsForRoom()
        {
            return "success=1&playlistIDs=4,7&b=r";
        }
        
        [HttpGet("api/php/getBinTunes.php")]
        public string GetBinTunes()
        {
            return "success=1&title0=Party+On&filename0=partyloop1&price0=50&artist0=Bin+Party&trackID0=1&title1=Party+Now&filename1=partyloop2&price1=50&artist1=Bin+Party&trackID1=2&title2=Party+Slow&filename2=partyloop3&price2=50&artist2=Bin+Party&trackID2=3&title3=Funk+Party&filename3=partyloop4&price3=50&artist3=Bin+Party&trackID3=4&title4=Fiesta+Tunes&filename4=fiesta_DanceAtTheRioGrande&price4=150&artist4=Bin+Party&trackID4=58&title5=Fall+In%2C+Flip+Out&filename5=celebrity%2FBE_Fall_In_Flip_Out_New_Master_06_03_12&price5=150&artist5=Bin+Party&trackID5=60&title6=Enemy&filename6=Enemy&price6=100&artist6=Dan+Morrissey&trackID6=5&title7=Altered+States&filename7=AlteredStates&price7=100&artist7=James+Treweek&trackID7=6&title8=Diva+Fever&filename8=DivaFever&price8=100&artist8=Redfor-Courtie&trackID8=7&title9=Scratch&filename9=Scratch&price9=100&artist9=Tim+Butler&trackID9=8&title10=Prodigious+Party&filename10=prodigious&price10=80&artist10=Prodigious&trackID10=9&numBinTunes=11&myTrack0=58&myTrack1=60&myTrack2=1&myTrack3=2&myTrack4=3&myTrack5=5&myTrack6=6&myTrack7=8&myTrack8=9&myTrack9=7&myTrack10=4&numTracksPurchased=11&b=r";
        }
    }
}