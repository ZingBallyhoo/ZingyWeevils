using ArcticFox.RPC.AmfGateway;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Amf;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Controllers
{
    public class WeevilKartAmfService
    {
        private readonly WeevilDBContext m_dbContext;
        private readonly WeevilWheelsSettings m_settings;
        
        public WeevilKartAmfService(WeevilDBContext dbContext, IOptionsSnapshot<WeevilWheelsSettings> settings)
        {
            m_dbContext = dbContext;
            m_settings = settings.Value;
        }
        
        public async Task<SubmitLapTimesResponse> SubmitLapTimes(AmfGatewayContext context, SubmitLapTimesRequest request)
        {
            return new SubmitLapTimesResponse
            {
                m_pbLap1 = request.m_lap1,
                m_pbLap2 = request.m_lap2,
                m_pbLap3 = request.m_lap3,
                m_pbTotal = request.m_lap1 + request.m_lap2 + request.m_lap3,
                
                m_unlock = true,
                m_medalInfo = new SubmitLapTimesResponse.MedalInfo
                {
                    m_hasWonMedal = true,
                    m_medalType = "gold",
                    m_color = 0xFFFFB0
                }
            };
        }
    }
}