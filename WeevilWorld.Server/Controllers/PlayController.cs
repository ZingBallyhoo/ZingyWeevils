using Microsoft.AspNetCore.Mvc;

namespace WeevilWorld.Server.Controllers
{
    [ApiController]
    [Route("play__")]
    public class PlayController : Controller
    {
        // GET
        [HttpGet("get-version")]
        public ActionResult GetVersion()
        {
            return Ok("{\"server\":\"127.0.0.1/ws\"}");
        }
    }
}