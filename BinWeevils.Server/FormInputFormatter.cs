using System.Net.Mime;
using System.Reflection;
using System.Text;
using ArcticFox.PolyType.FormEncoded;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace BinWeevils.Server
{
    public class FormInputFormatter : TextInputFormatter
    {
        public FormInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeNames.Application.FormUrlEncoded);
            
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }
        
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
            var bodyText = await reader.ReadToEndAsync();
            
            var options = new FormOptions();
                
            var polyTypeMethod = typeof(FormOptions)
                .GetMethod(nameof(FormOptions.Deserialize), BindingFlags.Instance | BindingFlags.Public, [typeof(string)])!
                .MakeGenericMethod(context.ModelType);
            var result = polyTypeMethod.Invoke(options, [bodyText]);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}