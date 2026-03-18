using System.Text.Json;
using Microsoft.Extensions.FileProviders;

namespace BinWeevils.Server.Services
{
    public class TrackRepository
    {
        private readonly IFileProvider m_fileProvider;
        public readonly Dictionary<int, TrackArchiveJson> m_tracks;
        
        public TrackRepository(IFileProvider fileProvider)
        {
            m_fileProvider = fileProvider;
            
            var tracksPath = Path.Combine("Data", "tracks.json");
            m_tracks = JsonSerializer.Deserialize<Dictionary<int, TrackArchiveJson>>(File.ReadAllText(tracksPath))!;
            foreach (var trackPair in m_tracks)
            {
                trackPair.Value.m_id = trackPair.Key;
            }
        }
        
        public class TrackArchiveJson
        {
            public int m_id;
            public string m_file { get; set; } 
            public string m_title { get; set; } 
            public string m_artist { get; set; } 
        }
        
        public bool IsBinTune(int id)
        {
            if (!m_tracks.TryGetValue(id, out var track))
            {
                return false;
            }
            return IsBinTune(track);
        }
        
        public bool IsBinTune(TrackArchiveJson track)
        {
            if (track.m_id == 53) return false; // duplicate of "fall in, flip out"
            
            var previewFileName = $"{track.m_file}_prev.mp3";
            var previewFileInfo = m_fileProvider.GetFileInfo(previewFileName);

            // if there is no preview file, this can't be a bin tune
            return previewFileInfo.Exists;
        }
    }
}