using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using ArcticFox.PolyType.FormEncoded;
using PolyType;

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
        
        public static async Task<TResp> PostSimpleFormAsync<TReq, TResp>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string uri, TReq body) where TReq : IShapeable<TReq> where TResp : IShapeable<TResp>
        {
            var response = await client.PostFormAsync(uri, body);
            return await response.DecodeFormResponse<TResp>();
        }
    }
}