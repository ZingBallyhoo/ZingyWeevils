using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BinWeevils.Tests.Integration
{
    public class IntegrationAuthStorage
    {
        public string? UserName { get; set; }
    }
    
    public class IntegrationAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public IntegrationAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authStorage = Context.RequestServices.GetRequiredService<IntegrationAuthStorage>();
            var userName = authStorage.UserName;
            
            if (userName == null)
            {
                throw new InvalidDataException("test username not specified");
            }
            
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, userName)
            ], Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}