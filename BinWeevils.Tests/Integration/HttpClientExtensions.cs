using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using ArcticFox.PolyType.Amf;
using ArcticFox.PolyType.Amf.Packet;
using ArcticFox.PolyType.FormEncoded;
using BinWeevils.Protocol.Form.Nest;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server.Controllers;
using PolyType;
using StackXML;

namespace BinWeevils.Tests.Integration
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostFormAsync<T>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string uri, T body) where T : IShapeable<T>
        {
            var serializedRequest = FormOptions.Default.Serialize(body);
            
            var content = new StringContent(serializedRequest, MediaTypeHeaderValue.Parse(MediaTypeNames.Application.FormUrlEncoded));
            return await client.PostAsync(uri, content);
        }
        
        public static async Task<T> DecodeFormResponse<T>(this HttpResponseMessage message) where T : IShapeable<T>
        {
            message.EnsureSuccessStatusCode();
            
            var str = await message.Content.ReadAsStringAsync();
            return FormOptions.Default.Deserialize<T>(str);
        }
        
        public static async Task PostSimpleFormAsync<TReq>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string uri, TReq body) where TReq : IShapeable<TReq>
        {
            // no response body
            var response = await client.PostFormAsync(uri, body);
            response.EnsureSuccessStatusCode();
        }
        
        public static async Task<TResp> PostSimpleFormAsync<TReq, TResp>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string uri, TReq body) where TReq : IShapeable<TReq> where TResp : IShapeable<TResp>
        {
            var response = await client.PostFormAsync(uri, body);
            return await response.DecodeFormResponse<TResp>();
        }
        
        public static async Task<TResp> PostAmfAsync<TReq, TResp>(this HttpClient client, AmfOptions options, string targetUri, TReq request) where TReq : IShapeable<TReq> where TResp : IShapeable<TResp>
        {
            var requestParamsArray = ArrayMapper.ToArray(request);
            var requestMessage = new AmfMessage
            {
                m_targetUri = targetUri,
                m_responseUri = "/1",
                m_data = requestParamsArray
            };
            var requestPacket = new AmfPacket
            {
                m_messages = [requestMessage]
            };
            
            var body = AmfPolyType.Serialize(requestPacket, options, GatewayShapeWitness.ShapeProvider);
            var bodyContent = new ByteArrayContent(body);
            bodyContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-amf");
            
            var httpResponse = await client.PostAsync("api/php/amfphp/gateway.php", bodyContent);
            httpResponse.EnsureSuccessStatusCode();
            
            var responseBody = await httpResponse.Content.ReadAsByteArrayAsync();
            var responsePacket = AmfPolyType.Deserialize<AmfPacket>(responseBody, options, GatewayShapeWitness.ShapeProvider);
            var responseMessage = responsePacket.m_messages.Single();
            
            return (TResp)responseMessage.m_data!;
        }
        
        public static async Task<StoredItems> GetStoredItems(this HttpClient client, string username)
        {
            var storedItemsResp = await client.PostFormAsync("api/nest/get-stored-items", new GetStoredItemsRequest
            {
                m_userID = username,
                m_mine = true
            });
            storedItemsResp.EnsureSuccessStatusCode();
            return XmlReadBuffer.ReadStatic<StoredItems>(await storedItemsResp.Content.ReadAsStringAsync());
        }
    }
}