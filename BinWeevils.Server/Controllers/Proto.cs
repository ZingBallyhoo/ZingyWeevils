using System.Net.Mime;
using BinWeevils.Protocol.Form;
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
    }
}