using System.Net.Mime;
using System.Text.Json;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TrackDetailsController : Controller
    {
        private readonly Dictionary<int, TrackArchiveJson> m_tracks;
        
        public TrackDetailsController(IConfiguration configuration)
        {
            var archivePath = configuration["ArchivePath"];
            
            // todo: well, this wont work in production
            var tracksPath = Path.Combine(archivePath, "..", "other", "tracks.json");
            m_tracks = JsonSerializer.Deserialize<Dictionary<int, TrackArchiveJson>>(System.IO.File.ReadAllText(tracksPath))!;
        }
        
        private class TrackArchiveJson
        {
            public string m_file { get; set; } 
            public string m_title { get; set; } 
            public string m_artist { get; set; } 
        }
        
        [StructuredFormPost("php/getTrackDetails.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public TrackDetailsResponse GetTrackDetails([FromBody] TrackDetailsRequest request)
        {
            if (!m_tracks.TryGetValue(request.m_trackID, out var track))
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
    }
}