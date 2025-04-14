using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
using ByteDev.FormUrlEncoded;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class Proto : Controller
    {
        private readonly IConfiguration m_configuration;
        
        public Proto(IConfiguration configuration)
        {
            m_configuration = configuration;
        }
        
        [StructuredFormPost("binConfig/{cluster}/checkVersion.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public CheckVersionResponse CheckVersion([FromBody] CheckVersionRequest r)
        {
            // (use 29 for trace, but its really spammy)
            // cluster is set to "h" in PlayGamePartial as it causes the camera & tv towers to be disabled
            // neither work anyway
            return new CheckVersionResponse
            {
                m_ok = 1,
                m_coreVersionNumber = 30,
                m_vodPlayerVersion = 15,
                m_vodContentVersion = 2,
            };
        }
        
        [StructuredFormPost("php/getAdPaths.php")]
        [StructuredFormPost("binConfig/{cluster}/getAdPaths.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public AdPathsResponse GetAdPaths([FromBody] AdPathsRequest request)
        {
            //if (request.m_area == AdPathsArea.LOADER)
            //{
            //    return new AdPathsResponse
            //    {
            //        m_paths = ["1", "2"]
            //    };
            //}
            return new AdPathsResponse();
        }
        
        [HttpGet("binConfig/config.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public SiteConfig GetConfig()
        {
            const string baseUrl = "http://localhost:80/";
            return new SiteConfig
            {
                m_domain = $"{baseUrl}",
                m_allowMultipleLogins = "true",
                m_servicesLocation = $"{baseUrl}api/",
                m_restrictFlashPlayers = "true",
                m_basePathSmall = $"{baseUrl}cdn/",
                m_basePathLarge = $"{baseUrl}cdn/",
                m_pathItemConfigs = $"{baseUrl}cdn/users/",
                m_pathAssetsNest = $"{baseUrl}cdn/users/",
                m_pathAssetsTycoon = $"{baseUrl}cdn/users/",
                m_pathAssetsGarden = $"{baseUrl}cdn/assetsGarden/",
                m_pathAssets3D = $"{baseUrl}cdn/assets3D/"
            };
        }
        
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
        
        [HttpGet("binConfig/getFile/0/locationDefinitions.xml")]
        [HttpGet("binConfig/getFile/0/{cluster}/locationDefinitions.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetLocationDefinitions()
        {
            return Results.File(m_configuration["LocationDefinitions"]!);
        }
        
        [HttpGet("binConfig/getFile/0/nestLocDefs.xml")]
        [HttpGet("binConfig/getFile/0/{cluster}/nestLocDefs.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetNestLocationDefinitions()
        {
            return Results.File(m_configuration["NestLocationDefinitions"]!);
        }
        
        [HttpGet("api/php/getTreasureHunt.php")]
        public string GetTreasureHunt()
        {
            return "responseCode=2&failed=0";
        }
        
        [HttpGet("api/php/getQuestData.php")] // old
        public string GetQuestData()
        {
            return "tasks=&itemList=&b=r&responseCode=1";
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
        
        [HttpPost("api/php/getSpecialMoves.php")]
        public string GetSpecialMoves()
        {
            return "responseCode=1&result=23";
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
        
        public class GeoResponse
        {
            [FormUrlEncodedPropertyName("l")] public string m_l { get; set; }
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
    }
}