using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WeevilWorld.Server.Controllers
{
    [ApiController]
    [Route("ad___")]
    public class AdController : Controller
    {
        public record AdData
        {
            [JsonPropertyName("seen_tracking")] public string m_trackingID { get; init; }
            [JsonPropertyName("path")] public string m_path { get; init; }
        }
        
        [HttpGet]
        public ActionResult GetAd()
        {
            return Ok(new AdData
            {
                m_trackingID = "angusIsTheBest",
                m_path = "angus.png"
            });
        }
    }
}