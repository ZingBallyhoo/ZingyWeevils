using ArcticFox.RPC.AmfGateway;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
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
            using var activity = ApiServerObservability.StartActivity("WeevilKartAmfService.SubmitLapTimes");
            activity?.SetTag("lap1", request.m_lap1);
            activity?.SetTag("lap2", request.m_lap2);
            activity?.SetTag("lap3", request.m_lap3);
            activity?.SetTag("trackID", request.m_trackID);
            
            if (request.m_userID != context.m_httpContext.User.Identity!.Name)
            {
                throw new InvalidDataException("submitting lap times for somebody else");
            }
            if (request.m_lap1 <= 5000 || request.m_lap2 <= 5000 || request.m_lap3 <= 5000)
            {
                throw new InvalidDataException("invalid lap time");
            }
            if (!m_settings.Tracks.TryGetValue(request.m_trackID, out var track))
            {
                throw new InvalidDataException("unknown track");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            var dto = await m_dbContext.GetIdxAndNestID(request.m_userID);
            
            var totalTime = TimeSpan.FromMilliseconds(request.m_lap1 + request.m_lap2 + request.m_lap3);
            var highestTrophy = GetTrophyForTime(totalTime, track);
            var awardedTrophy = false;

            for (var trophy = highestTrophy; trophy > WeevilWheelsTrophyType.None; trophy--)
            {
                if (await TryAwardTrophy(trophy, track, dto.m_nestID))
                {
                    awardedTrophy = true;
                }
            }
            
            // todo: can't use upsert because we need the old record...
            var pbFilter = m_dbContext.m_trackPersonalBests
                .Where(x => x.m_weevilIdx == dto.m_idx)
                .Where(x => x.m_gameType == request.m_trackID);
            var currentPB = await pbFilter.SingleOrDefaultAsync();
            
            if (currentPB == null)
            {
                await m_dbContext.m_trackPersonalBests.AddAsync(new WeevilTrackPersonalBestDB
                {
                    m_weevilIdx = dto.m_idx,
                    m_gameType = request.m_trackID,
                    m_lap1 = request.m_lap1,
                    m_lap2 = request.m_lap2,
                    m_lap3 = request.m_lap3,
                });
                await m_dbContext.SaveChangesAsync();
            } else if (currentPB.m_total > totalTime.TotalSeconds)
            {
                // doesn't matter if there's a race here really...
                // it would be your own fault
                var rowsUpdated = await pbFilter
                    .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_lap1, request.m_lap1)
                    .SetProperty(x => x.m_lap2, request.m_lap2)
                    .SetProperty(x => x.m_lap3, request.m_lap3)
                );
                
                if (rowsUpdated == 0)
                {
                    throw new Exception("race updating pb");
                }
            }
            
            await transaction.CommitAsync();
            
            return new SubmitLapTimesResponse
            {
                m_pbLap1 = currentPB?.m_lap1 ?? 0,
                m_pbLap2 = currentPB?.m_lap2 ?? 0,
                m_pbLap3 = currentPB?.m_lap3 ?? 0,
                m_pbTotal = currentPB?.m_total ?? 0,
                
                m_unlock = false,
                m_medalInfo = new SubmitLapTimesResponse.MedalInfo
                {
                    m_hasWonMedal = awardedTrophy,
                    m_medalType = awardedTrophy ? highestTrophy.ToString().ToLowerInvariant() : "",
                    m_color = awardedTrophy ? m_settings.TrophyColors[highestTrophy] : 0
                }
            };
        }
        
        private async Task<bool> TryAwardTrophy(WeevilWheelsTrophyType trophy, WeevilWheelsTrackSettings track, uint nestID)
        {
            var itemTypeID = await m_dbContext.FindItemByConfigName(track.TrophyItem);
            if (itemTypeID == null) throw new Exception($"unable to find trophy item; {track.TrophyItem}");
            
            var color = new ItemColor(m_settings.TrophyColors[trophy]);
            
            var itemFilter = m_dbContext.m_nestItems
                .Where(x => x.m_nestID == nestID)
                .Where(x => x.m_itemTypeID == itemTypeID.Value)
                .Where(x => 
                    x.m_color.m_r == color.m_r && 
                    x.m_color.m_g == color.m_g && 
                    x.m_color.m_b == color.m_b);
            
            if (await itemFilter.AnyAsync())
            {
                // already has
                return false;
            }
            
            var item = new NestItemDB
            {
                m_nestID = nestID,
                m_itemTypeID = itemTypeID.Value,
                m_color = color
            };
            await m_dbContext.m_nestItems.AddAsync(item);
            await m_dbContext.SaveChangesAsync();
            
            if (await itemFilter.CountAsync() != 1)
            {
                throw new InvalidDataException("race while awarding trophy");
            }
            
            return true;
        }
        
        private static WeevilWheelsTrophyType GetTrophyForTime(TimeSpan time, WeevilWheelsTrackSettings track)
        {
            for (var type = WeevilWheelsTrophyType.Count - 1; type >= 0; type--)
            {
                if (!track.TrophyTimes.TryGetValue(type, out var trophyTime)) continue;
                
                if (time <= trophyTime)
                {
                    return type;
                }
            }
            return WeevilWheelsTrophyType.None;
        }
    }
}