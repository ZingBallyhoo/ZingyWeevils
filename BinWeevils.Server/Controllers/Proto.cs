using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class Proto : Controller
    {
        [StructuredFormEndpoint]
        [HttpPost("binConfig/{cluster}/checkVersion.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public CheckVersionResponse CheckVersion([FromBody] CheckVersionRequest r)
        {
            Console.Out.WriteLine(r.m_siteVersion);
            return new CheckVersionResponse
            {
                m_ok = 1,
                m_coreVersionNumber = 18
            };
        }
        
        [HttpGet("test")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public CheckVersionResponse CheckVersion2()
        {
            return new CheckVersionResponse();
        }
        
        [StructuredFormEndpoint]
        [HttpPost("binConfig/uk/getAdPaths.php")]
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
                m_servicesLocation = $"{baseUrl}",
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
    }
}