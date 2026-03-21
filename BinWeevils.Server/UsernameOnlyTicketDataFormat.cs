using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BinWeevils.Server
{
    public class UsernameOnlyTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        public const string Scheme = "UsernameOnly2";
        
        public string Protect(AuthenticationTicket data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            if (data.AuthenticationScheme != Scheme)
            {
                throw new InvalidDataException($"unsupported scheme: {data.AuthenticationScheme}");
            }
            return data.Principal.FindFirstValue(ClaimTypes.Name)!;
        }

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Name, protectedText!)
            ]));
            return new AuthenticationTicket(principal, Scheme);
        }
    }
}