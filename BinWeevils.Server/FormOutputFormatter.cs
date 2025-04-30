using System.Net.Mime;
using System.Reflection;
using System.Text;
using ArcticFox.PolyType.FormEncoded;
using Microsoft.AspNetCore.Mvc.Formatters;
using PolyType;

namespace BinWeevils.Server.Services
{
    public partial class FormOutputFormatter : TextOutputFormatter
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

            string encoded;
            if (context.ObjectType == typeof(Dictionary<string, string>))
            {
                encoded = options.Serialize2<Dictionary<string, string>, DictWitness>((Dictionary<string, string>?)context.Object);
            } else
            {
                var polyTypeMethod = typeof(FormOptions)
                    .GetMethod(nameof(FormOptions.Serialize), BindingFlags.Instance | BindingFlags.Public)!
                    .MakeGenericMethod(context.ObjectType!);
                encoded = (string)polyTypeMethod.Invoke(options, [context.Object])!;
            }
            
            
            return context.HttpContext.Response.WriteAsync(encoded, selectedEncoding);
        }
        
        [GenerateShape<Dictionary<string, string>>]
        private partial class DictWitness
        {
        }
    }
}