using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using StackXML;

namespace BinWeevils.Server
{
    public class StackXMLOutputFormatter : TextOutputFormatter
    {
        public StackXMLOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeNames.Application.Xml);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type? type)
        {
            if (type == null) return false;
            return type.IsAssignableTo(typeof(IXmlSerializable));
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            using var activity = ApiServerObservability.StartActivity("StackXMLOutputFormatter.WriteResponseBodyAsync");
            activity?.SetTag("modelType", context.ObjectType);
            
            var encoded = XmlWriteBuffer.SerializeStatic((IXmlSerializable)context.Object!, CDataMode.Off);
            return context.HttpContext.Response.WriteAsync(encoded, selectedEncoding);
        }
    }
}