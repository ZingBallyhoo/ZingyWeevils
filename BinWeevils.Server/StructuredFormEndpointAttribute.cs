using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BinWeevils.Server
{
    public class StructuredFormEndpointAttribute : TypeFilterAttribute
    {
        public StructuredFormEndpointAttribute() : base(typeof(DisableFormEndpointFilter))
        {
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