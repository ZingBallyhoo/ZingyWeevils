using ArcticFox.RPC.AmfGateway;
using BinWeevils.Protocol.Amf;

namespace BinWeevils.Server.Controllers
{
    public class PetAmfService
    {
        public async Task<int> GetPetCount(AmfGatewayContext context, string userID)
        {
            return 0;
        }
        
        public async Task<bool> ValidatePetName(AmfGatewayContext context, ValidatePetNameRequest request)
        {
            return true;
        }
    }
}