using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BinWeevils.Server
{
    public class StructuredFormPostAttribute : TypeFilterAttribute, IRouteTemplateProvider, IActionHttpMethodProvider
    {
        public string? Template { get; }
        int? IRouteTemplateProvider.Order => 0;
        string? IRouteTemplateProvider.Name => null;
        IEnumerable<string> IActionHttpMethodProvider.HttpMethods => ["POST"];
        
        public StructuredFormPostAttribute([StringSyntax("Route")] string template) : base(typeof(DisableFormEndpointFilter))
        {
            Template = template;
        }
        
        private class DisableFormEndpointFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                context.HttpContext.Features.Set<IFormFeature>(new DisabledFormFeature());
            }
        }
        
        private class DisabledFormFeature : IFormFeature
        {
            public IFormCollection ReadForm()
            {
                throw new NotImplementedException();
            }

            public Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public bool HasFormContentType => false;

            public IFormCollection? Form 
            { 
                get => throw new InvalidDataException(); 
                set => throw new InvalidDataException(); 
            }
        }
    }
}