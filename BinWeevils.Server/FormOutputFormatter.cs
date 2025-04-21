using System.Net.Mime;
using System.Text;
using ByteDev.FormUrlEncoded;
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
            var encoded = FormUrlEncodedSerializer.Serialize(context.Object, new SerializeOptions
            {
                EncodeSpaceAsPlus = false
            });
            return context.HttpContext.Response.WriteAsync(encoded, selectedEncoding);
        }
    }
}