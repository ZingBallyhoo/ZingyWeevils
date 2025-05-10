using System.Text.Json.Serialization;

namespace BinWeevils.Protocol.Json
{
    public record struct JsonJugglingTrick
    {
        [JsonPropertyName("id")] public uint m_id { get; set; }
        [JsonPropertyName("aptitude")] public double m_aptitude { get; set; }
        [JsonPropertyName("numBalls")] public byte m_numBalls { get; set; }
        [JsonPropertyName("pattern")] public string m_pattern { get; set; }
        [JsonPropertyName("difficulty")] public uint m_difficulty { get; set; }
        [JsonPropertyName("name")] public string m_name { get; set; }
    }
}