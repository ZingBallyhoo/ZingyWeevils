using System.Diagnostics;

namespace BinWeevils.Server
{
    public class UsernameTracingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var userName = context.User.Identity?.Name;
            if (userName != null)
            {
                Activity.Current?.SetTag("userName", userName);
            }
            
            await next(context);
        }
    }
}