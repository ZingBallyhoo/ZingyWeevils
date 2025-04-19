using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace WeevilWorld.Server
{
    public static class HttpContextExtensions
    {
        public static string AddFileVersionToPath(this HttpContext context, string path)
        {
            return context
                .RequestServices
                .GetRequiredService<IFileVersionProvider>()
                .AddFileVersionToPath(context.Request.PathBase, path);
        }
        
        public static void CacheResponse(this StaticFileResponseContext context, TimeSpan duration)
        {
            var typedHeaders = context.Context.Response.GetTypedHeaders();
            typedHeaders.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = duration
            };
            typedHeaders.Expires = new DateTimeOffset(DateTime.UtcNow).Add(duration);
        }
    }
}