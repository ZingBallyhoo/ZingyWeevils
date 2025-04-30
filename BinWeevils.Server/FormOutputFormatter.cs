using System.Net.Mime;
using System.Reflection;
using System.Text;
using ArcticFox.PolyType.FormEncoded;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace BinWeevils.Server
{
    public class FormOutputFormatter : TextOutputFormatter
    {
        public FormOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeNames.Application.FormUrlEncoded);
            
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }
        
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var options = new FormOptions();

            var polyTypeMethod = typeof(FormOptions)
                .GetMethod(nameof(FormOptions.Serialize), BindingFlags.Instance | BindingFlags.Public)!
                .MakeGenericMethod(context.ObjectType!);
            var encoded = (string)polyTypeMethod.Invoke(options, [context.Object])!;
            
            return context.HttpContext.Response.WriteAsync(encoded, selectedEncoding);
        }
    }
}