using System.Net.Mime;
using System.Reflection;
using System.Text;
using ByteDev.FormUrlEncoded;
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
            
            var method = typeof(FormUrlEncodedSerializer).GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public, [typeof(string)])!
                .MakeGenericMethod(context.ModelType);
            
            var bodyText = await reader.ReadToEndAsync();
            var result = method.Invoke(null, [bodyText]);
            
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}