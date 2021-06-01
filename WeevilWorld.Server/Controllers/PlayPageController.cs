using Microsoft.AspNetCore.Mvc;
using WeevilWorld.Server.Pages;

namespace WeevilWorld.Server.Controllers
{
    public class PlayPageController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View("~/Pages/PlayPage.cshtml", new PlayPageModel
            {
                BuildName = "v268",
                BuildJsonFilename = "04"
            });
        }
        
        [HttpGet("/index2")]
        public IActionResult IndexWonderPark()
        {
            return View("~/Pages/PlayPage.cshtml", new PlayPageModel
            {
                BuildName = "WonderPark_final",
                BuildJsonFilename = "WebGLBuilds"
            });
        }
    }
}