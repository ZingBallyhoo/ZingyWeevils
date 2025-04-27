using System.Text.Json.Serialization;

namespace BinWeevils.Protocol.Json
{
    public class AvailablePalettes
    {
        [JsonPropertyName(("responseCode"))] public int m_responseCode { get; set; }
        [JsonPropertyName("palettes")] public Dictionary<uint, List<string>> m_palette { get; set; }
    }
}