using System.Security.Claims;
using BinWeevils.Common.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PolyType;

namespace BinWeevils.Server.Controllers
{
    public partial class IndexController : Controller
    {
        private readonly UserManager<WeevilAccount> m_identityManager;
        
        public IndexController(UserManager<WeevilAccount> identityManager)
        {
            m_identityManager = identityManager;
        }
        
        [HttpGet("index.php")]
        public IResult IndexRedirect()
        {
            return Results.Redirect("/", permanent: true);
        }
        
        [StructuredFormPost("")]
        public async Task<IResult> PostUsername([FromBody] UsernameForm form)
        {
            var username = form.m_username.Trim().Replace('+', ' ');
            if (string.IsNullOrWhiteSpace(username))
            {
                return Results.Redirect("/");
            }
            
            // look up the username in the database to try and get the correct casing for the username
            // note: this can race if two people create the same normalized name at the same time
            var foundUser = await m_identityManager.FindByNameAsync(username);
            if (foundUser != null)
            {
                username = foundUser.UserName!;
            }
            
            // forward to whatever the default sign-in handler is
            // this principal has IsAuthenticated set to false
            // which means it would be rejected by standard handlers
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username)
            ]));
            
            // todo: to make a standard cookie persistent, use `AuthenticationProperties { IsPersistent = true}`
            await HttpContext.SignInAsync(principal);
            
            return Results.Redirect("/game.php");
        }
        
        [GenerateShape]
        public partial class UsernameForm
        {
            [PropertyShape(Name = "username")] public string m_username { get; set; }
        }
    }
}