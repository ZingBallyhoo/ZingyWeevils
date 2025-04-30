using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class ActiveZonesResponse
    {
        [PropertyShape(Name = "names")] public List<string> m_names { get; set; } = new List<string>();
        [PropertyShape(Name = "ips")] public List<string> m_ips { get; set; } = new List<string>();
        [PropertyShape(Name = "oo5")] public List<int> m_outOf5 { get; set; } = new List<int>();
        [PropertyShape(Name = "responseCode")] public bool m_responseCode { get; set; }
    }
}