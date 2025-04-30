using System.Text.Json;

namespace BinWeevils.Server.Services
{
    public class TrackRepository
    {
        public readonly Dictionary<int, TrackArchiveJson> m_tracks;
        private readonly string m_archivePathA;
        private readonly string m_archivePathB;
        
        public TrackRepository(IConfiguration configuration)
        {
            var tracksPath = Path.Combine("Data", "tracks.json");
            m_tracks = JsonSerializer.Deserialize<Dictionary<int, TrackArchiveJson>>(File.ReadAllText(tracksPath))!;
            foreach (var trackPair in m_tracks)
            {
                trackPair.Value.m_id = trackPair.Key;
            }
            
            m_archivePathA = Path.Combine(configuration["ArchivePath"]!, "bintunes");
            m_archivePathB = Path.Combine(configuration["ArchivePath"]!, "play", "bintunes");
        }
        
        public class TrackArchiveJson
        {
            public int m_id;
            public string m_file { get; set; } 
            public string m_title { get; set; } 
            public string m_artist { get; set; } 
        }
        
        public bool IsBinTune(TrackArchiveJson track)
        {
            if (track.m_id == 53) return false; // duplicate of "fall in, flip out"
            
            var fn = $"{track.m_file}_prev.mp3";
            return File.Exists(Path.Combine(m_archivePathA, fn)) ||
                   File.Exists(Path.Combine(m_archivePathB, fn));
        }
    }
}