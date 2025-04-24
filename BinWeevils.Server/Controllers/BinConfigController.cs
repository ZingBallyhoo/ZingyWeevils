using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class BinConfigController : Controller
    {
        private readonly IConfiguration m_configuration;
        
        public BinConfigController(IConfiguration configuration)
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
        
        [HttpGet("binConfig/config.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public SiteConfig GetConfig()
        {
            var referrerUrl = (string?)HttpContext.Request.Headers.Host ?? throw new InvalidDataException("no host header");
            var referrer = new Uri(referrerUrl);
            var baseUrl = $"{referrer.Scheme}://{referrer.Host}/";
            
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
        
        [HttpGet("binConfig/getFile/0/locationDefinitions.xml")]
        [HttpGet("binConfig/getFile/0/{cluster}/locationDefinitions.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetLocationDefinitions()
        {
            return Results.File(Path.GetFullPath(m_configuration["LocationDefinitions"]!));
        }
        
        [HttpGet("binConfig/getFile/0/nestLocDefs.xml")]
        [HttpGet("binConfig/getFile/0/{cluster}/nestLocDefs.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetNestLocationDefinitions()
        {
            return Results.File(m_configuration["NestLocationDefinitions"]!);
        }
        
        [StructuredFormPost("php/getAdPaths.php")]
        [StructuredFormPost("binConfig/{cluster}/getAdPaths.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public object GetAdPaths([FromBody] AdPathsRequest request)
        {
            if (request.m_area == AdPathsArea.LOADER)
            {
                return new LoaderAdPathsResponse
                {
                    m_paths = ["Uni.swf", "Panko.swf", "Hired.swf", "Stapler.swf"]
                };
            }
            if (request.m_area == AdPathsArea.OUTSIDE_SHOPPING_MALL)
            {
                // left(us) = ad1
                // right = ad2
                return new TwoAdPathsResponse();
            }
            return new TwoAdPathsResponse();
        }
    }
}