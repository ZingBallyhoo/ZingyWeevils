using System.Dynamic;
using ArcticFox.PolyType.Amf;
using ArcticFox.PolyType.Amf.Packet;
using BinWeevils.Protocol.Amf;
using Microsoft.AspNetCore.Mvc;
using PolyType;

namespace BinWeevils.Server.Controllers
{
    [ApiController]
    [Route("php/amfphp")]
    public class AmfController : Controller
    {
        [HttpPost("gateway.php")]
        [Consumes("application/x-amf")]
        [Produces("application/x-amf")]
        [RequestSizeLimit(10000)]
        public async Task<IResult> Gateway(CancellationToken cancellationToken)
        {
            if (Request.ContentLength == null) return Results.InternalServerError();
            
            var body = new byte[Request.ContentLength.Value];
            await Request.Body.ReadExactlyAsync(body, cancellationToken);
            
            var packet = AmfPolyType.Deserialize<AmfPacket, GatewayShapeWitness>(body, AmfOptions.Default);
            var message = packet.m_messages.Single();
            
            AmfPacket response;
            if (message.m_targetUri == "weevilservices.cWeevilLoginService.getLoginDetails")
            {
                var random = Random.Shared.Next(0, 9999999);
                
                response = new AmfPacket
                {
                    m_messages = new List<AmfMessage>
                    {
                        new AmfMessage
                        {
                            m_targetUri = $"{message.m_responseUri}/onResult",
                            m_responseUri = "null",
                            m_data = new GetLoginDetailsResponse
                            {
                                m_userName = $"joe{random}",
                                m_userIdx = random,
                                m_tycoon = 1,
                                m_loginKey = "secretjoe"
                            }
                        }
                    }
                };
            } else if (message.m_targetUri == "weevilservices.cWeevilLoginService.getUserBuddyCount")
            {
                response = new AmfPacket
                {
                    m_messages = new List<AmfMessage>
                    {
                        new AmfMessage
                        {
                            m_targetUri = $"{message.m_responseUri}/onResult",
                            m_responseUri = "null",
                            m_data = "-1"
                        }
                    }
                };
            } else
            {
                throw new Exception($"unknown target: {message.m_targetUri}");
            }
            
            var ser = AmfPolyType.Serialize<AmfPacket, GatewayShapeWitness>(response, AmfOptions.Default);
            
            //var deSer = AmfPolyType.Deserialize<AmfPacket, GatewayShapeWitness>(ser);
            
            return Results.Bytes(ser);
        }
    }
        
    [GenerateShape<AmfPacket>]
    [GenerateShape<GetLoginDetailsResponse>]
    [GenerateShape<object[]>]
    [GenerateShape<double>]
    [GenerateShape<ExpandoObject>]
    internal partial class GatewayShapeWitness;
}