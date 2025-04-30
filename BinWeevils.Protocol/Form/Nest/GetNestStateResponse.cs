using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class GetNestStateResponse
    {
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
        [PropertyShape(Name = "err")] public string m_error { get; set; }
        [PropertyShape(Name = "xp")] public uint m_weevilXp { get; set; }
        [PropertyShape(Name = "score")] public int m_score { get; set; }
        [PropertyShape(Name = "fuel")] public uint m_fuel { get; set; }
        [PropertyShape(Name = "lastUpdate")] public string m_lastUpdate { get; set; }
    }
}