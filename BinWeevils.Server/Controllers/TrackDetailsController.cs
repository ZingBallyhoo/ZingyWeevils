using System.Net.Mime;
using BinWeevils.Protocol.Form;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TrackDetailsController : Controller
    {
        private readonly TrackRepository m_repository;
        
        public TrackDetailsController(TrackRepository repository) 
        {
            m_repository = repository;
        }
        
        [StructuredFormPost("php/getTrackDetails.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public TrackDetailsResponse GetTrackDetails([FromBody] TrackDetailsRequest request)
        {
            if (!m_repository.m_tracks.TryGetValue(request.m_trackID, out var track))
            {
                return new TrackDetailsResponse
                {
                    m_responseCode = 2
                };
            }
            
            return new TrackDetailsResponse
            {
                m_responseCode = 1,
                m_file = track.m_file,
                m_title = track.m_title,
                m_artist = track.m_artist,
                m_trackID = request.m_trackID
            };
        }
        
        [HttpGet("php/getBinTunes.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public Dictionary<string, string> GetBinTunes()
        {
            var binTunes = m_repository.m_tracks.Values.Where(x => m_repository.IsBinTune(x)).ToArray();
            var resp = new Dictionary<string, string>
            {
                {"success", "1"},
                {"numBinTunes", $"{binTunes.Length}"},
                {"numTracksPurchased", $"{binTunes.Length}"}
            };
            
            for (var i = 0; i < binTunes.Length; i++)
            {
                resp.Add($"title{i}", binTunes[i].m_title);
                resp.Add($"filename{i}", binTunes[i].m_file);
                resp.Add($"price{i}", "0");
                resp.Add($"artist{i}", binTunes[i].m_artist);
                resp.Add($"trackID{i}", $"{binTunes[i].m_id}");
                
                resp.Add($"myTrack{i}", $"{binTunes[i].m_id}");
            }
            
            return resp;
        }
    }
}