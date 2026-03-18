using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

namespace BinWeevils.Server.Services
{
    public class TrackRepository
    {
        private readonly Dictionary<int, TrackArchiveJson> m_tracks;
        private readonly Dictionary<int, TrackArchiveJson> m_binTunes = [];
        
        public TrackRepository(IFileProvider fileProvider)
        {
            var tracksPath = Path.Combine("Data", "tracks.json");
            m_tracks = JsonSerializer.Deserialize<Dictionary<int, TrackArchiveJson>>(File.ReadAllText(tracksPath))!;
            
            foreach (var trackPair in m_tracks)
            {
                trackPair.Value.m_id = trackPair.Key;

                if (CheckIsBinTune(fileProvider, trackPair.Value))
                {
                    m_binTunes.Add(trackPair.Key, trackPair.Value);
                }
            }
            
            // 0 bin tunes will cause the plaza music ui to not open
            Debug.Assert(m_binTunes.Count != 0);
        }
        
        public class TrackArchiveJson
        {
            public int m_id;
            public string m_file { get; set; } 
            public string m_title { get; set; } 
            public string m_artist { get; set; } 
        }
        
        private static bool CheckIsBinTune(IFileProvider fileProvider, TrackArchiveJson track)
        {
            if (track.m_id == 53) return false; // duplicate of "fall in, flip out"
            
            var previewFileName = $"{track.m_file}_prev.mp3";
            var previewFilePath = Path.Combine("bintunes", previewFileName);
            var previewFileInfo = fileProvider.GetFileInfo(previewFilePath);

            // if there is no preview file, this can't be a bin tune
            return previewFileInfo.Exists;
        }
        
        public IEnumerable<TrackArchiveJson> GetTracks()
        {
            return m_tracks.Values;
        }

        public IEnumerable<TrackArchiveJson> GetBinTunes()
        {
            return m_binTunes.Values;
        }

        
        public bool TryGetTrack(int id, [NotNullWhen(true)] out TrackArchiveJson? track)
        {
            return m_tracks.TryGetValue(id, out track);
        }
        
        public bool IsBinTune(int id)
        {
            return m_binTunes.ContainsKey(id);
        }
    }
}