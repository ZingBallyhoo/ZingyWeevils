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
        public ActionResult GetAd(int area)
        {
            var adPath = area switch
            {
                1 => "WWBilboardADGeneric_658x275.png", // old street billboards
                2 => "WWBilboardADGeneric_658x275.png", // old street billboards
                _ => null
            };
            if (adPath == null)
            {
                return Ok(new {});
            }
            return Ok(new AdData
            {
                m_trackingID = "thisIsntARealTrackingID",
                m_path = adPath
            });
        }
    }
}