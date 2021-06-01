using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WeevilWorld.Server.Controllers
{
    [ApiController]
    [Route("play__")]
    public class PlayController : Controller
    {
        public record VersionData
        {
            [JsonPropertyName("server")] public string m_websocketUrl { get; init; }
        }
        
        [HttpGet("get-version")]
        public ActionResult GetVersion()
        {
            return Ok(new VersionData
            {
                m_websocketUrl = "ww.zingy.dev/ws"
            });
        }
    }
}