using Microsoft.AspNetCore.Mvc;
using PolyType;

namespace BinWeevils.Server.Controllers
{
    public partial class IndexController : Controller
    {
        [HttpGet("index.php")]
        public IResult IndexRedirect()
        {
            return Results.Redirect("/", permanent: true);
        }
        
        [StructuredFormPost("")]
        public IResult PostUsername([FromBody] UsernameForm form)
        {
            var username = form.m_username.Trim().Replace('+', ' ');
            if (string.IsNullOrWhiteSpace(username))
            {
                return Results.Redirect("/");
            }
            
            Response.Cookies.Append("username", username, new CookieOptions
            {
                Expires = DateTime.MaxValue
            });
            return Results.Redirect("/game.php");
        }
        
        [GenerateShape]
        public partial class UsernameForm
        {
            [PropertyShape(Name = "username")] public string m_username { get; set; }
        }
    }
}