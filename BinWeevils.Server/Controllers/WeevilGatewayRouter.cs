using ArcticFox.PolyType.Amf;
using ArcticFox.RPC.AmfGateway;
using BinWeevils.Protocol.Amf;
using PolyType;

namespace BinWeevils.Server.Controllers
{
    public class WeevilGatewayRouter : IAmfGatewayRouter
    {
        public async ValueTask<object> RouteRequest(AmfGatewayContext context, CancellationToken cancellationToken)
        {
            switch (context.m_message.m_targetUri)
            {
                case "weevilservices.cWeevilLoginService.getLoginDetails":
                {
                    var username = context.m_httpContext.User.Identity!.Name;
                    
                    return new GetLoginDetailsResponse
                    {
                        m_userName = username,
                        m_userIdx = 0, // todo: we can't get this here...
                        m_tycoon = 1,
                        m_loginKey = ""
                    };
                }
                case "weevilservices.cWeevilLoginService.getUserBuddyCount":
                {
                    var request = ArrayMapper.ToObject<GetUserBuddyCountRequest>(context.m_message.m_data);
                    return "-1";
                }
                
                case "weevilservices.cPetShop.getUserPetCount":
                {
                    var petService = context.m_httpContext.RequestServices.GetRequiredService<PetAmfService>();
                    var request = ArrayMapper.ToObject<GetUserPetCountRequest>(context.m_message.m_data);
                    return await petService.GetPetCount(context, request);
                }
                case "weevilservices.cPetShop.validatePetName":
                {
                    var petService = context.m_httpContext.RequestServices.GetRequiredService<PetAmfService>();
                    var request = ArrayMapper.ToObject<ValidatePetNameRequest>(context.m_message.m_data);
                    return await petService.ValidatePetName(context, request);
                }
                case "weevilservices.cPetShop.userBuyPet":
                {
                    var petService = context.m_httpContext.RequestServices.GetRequiredService<PetAmfService>();
                    var request = ArrayMapper.ToObject<BuyPetRequest>(context.m_message.m_data);
                    return await petService.BuyPet(context, request);
                }
                case "weevilservices.cPetShop.userBuyPetFood":
                {
                    var petService = context.m_httpContext.RequestServices.GetRequiredService<PetAmfService>();
                    var request = ArrayMapper.ToObject<BuyPetFoodRequest>(context.m_message.m_data);
                    return await petService.BuyPetFood(context, request);
                }
                
                case "weevilservices.cWeevilKartGameScores.submitSingleUserTimes":
                {
                    var kartService = context.m_httpContext.RequestServices.GetRequiredService<WeevilKartAmfService>();
                    var request = ArrayMapper.ToObject<SubmitLapTimesRequest>(context.m_message.m_data);
                    return await kartService.SubmitLapTimes(context, request);
                }
                
                default:
                {
                    throw new Exception($"Unknown AMF Target Uri: \"{context.m_message.m_targetUri}\"");
                }
            }
        }
    }
    
    [GenerateShape<object[]>]
    [GenerateShape<GetLoginDetailsResponse>]
    [GenerateShape<SubmitLapTimesResponse>]
    internal partial class GatewayShapeWitness;
    
    /*[RpcMethod(typeof(AmfPassthroughRpcMethod<object, GetLoginDetailsResponse>), "h")]
    public partial class LoginServiceAmf
    {
        
    }
    
    public class LoginServiceAmf_Impl : LoginServiceAmf<AmfGatewayContext>
    {
        public override async ValueTask<GetLoginDetailsResponse> h(AmfGatewayContext socket, object request, CancellationToken cancellationToken = default)
        {
            return null;
        }
    }
    
    public class AmfPassthroughRpcMethod<TReq, TResp> : RpcMethod<TReq, TResp>
    {
        public AmfPassthroughRpcMethod(string serviceName, string methodName) : base(serviceName, methodName)
        {
        }

        public override object DecodeRequest(ReadOnlySpan<byte> data, object? token)
        {
            return token!;
        }

        public override object DecodeResponse(ReadOnlySpan<byte> data, object? token)
        {
            return token!;
        }
    }*/
}