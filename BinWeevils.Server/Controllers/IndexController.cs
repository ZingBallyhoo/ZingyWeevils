using ByteDev.FormUrlEncoded;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    public class IndexController : Controller
    {
        [HttpGet("index.php")]
        public IResult IndexRedirect()
        {
            return Results.Redirect("/", permanent: true);
        }
        
        [StructuredFormPost("")]
        public IResult PostUsername([FromBody] UsernameForm form)
        {
            if (string.IsNullOrWhiteSpace(form.m_username))
            {
                return Results.Redirect("/");
            }
            
            Response.Cookies.Append("username", form.m_username);
            return Results.Redirect("/game.php");
        }
        
        public class UsernameForm
        {
            [FormUrlEncodedPropertyName("username")] public string m_username { get; set; }
        }
    }
}