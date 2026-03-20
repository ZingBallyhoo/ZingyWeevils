using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server
{
    public class UsernameOnlyAuthenticationHandler : SignInAuthenticationHandler<AuthenticationSchemeOptions>
    {
        public UsernameOnlyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Cookies.TryGetValue("username", out var username))
            {
                return Task.FromResult(AuthenticateResult.Fail("No username cookie"));
            }
            
            if (Activity.Current is {} currentActivity)
            {
                currentActivity.SetTag("userName", username);
            }
            
            // note: not passing the scheme to the principal
            // that would signify that this principal is authenticated, it's not
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username)
            ]));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            Context.Response.Cookies.Delete("username");
            return Task.CompletedTask;
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            Response.Cookies.Append("username", user.FindFirstValue(ClaimTypes.Name)!, new CookieOptions
            {
                Expires = DateTime.MaxValue
            });
            return Task.CompletedTask;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (Context.Request.Path == "/game.php")
            {
                Context.Response.Redirect("/");
                return Task.CompletedTask;
            }
            
            return base.HandleChallengeAsync(properties);
        }
    }
}