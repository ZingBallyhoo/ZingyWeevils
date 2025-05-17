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
        
        [HttpGet("api/weevil/remaining-revenue")]
        public string GetRemainingRevenue()
        {
            return "res=0";
        }
    }
}