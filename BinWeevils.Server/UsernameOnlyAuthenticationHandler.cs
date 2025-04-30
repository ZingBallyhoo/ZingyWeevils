using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server
{
    public class UsernameOnlyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public UsernameOnlyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Cookies.TryGetValue("username", out var username))
            {
                var random = Random.Shared.Next(0, 9999999);
                username = $"fairriver{random}";
                Context.Response.Cookies.Append("username", username);
            }
            
            if (Activity.Current is {} currentActivity)
            {
                currentActivity.SetTag("userName", username);
            }
            
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username)
            ], Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}