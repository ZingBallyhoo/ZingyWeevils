using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial record CheckVersionRequest
    {
        [PropertyShape(Name = "version")] public int m_siteVersion { get; set; }
    }
}