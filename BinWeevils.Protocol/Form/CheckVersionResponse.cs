using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial record CheckVersionResponse
    {
        [PropertyShape(Name = "vOK")] public int m_ok { get; set; }
        [PropertyShape(Name = "core")] public int m_coreVersionNumber { get; set; }
        [PropertyShape(Name = "VODplayer")] public int m_vodPlayerVersion { get; set; }
        [PropertyShape(Name = "VODcontent")] public int m_vodContentVersion { get; set; }
        [PropertyShape(Name = "locDef")] public int m_locDefVersion { get; set; }
        [PropertyShape(Name = "nestDef")] public int m_nestDefVersion { get; set; }
        [PropertyShape(Name = "p")] public int m_p { get; set; }
        [PropertyShape(Name = "binBadgesDisplay")] public int m_binBadgesDisplayVersion { get; set; }
        [PropertyShape(Name = "achievementAlerts")] public int m_achievementAlertsVersion { get; set; }
        [PropertyShape(Name = "URLDef")] public int m_urlDefVersion { get; set; }
        [PropertyShape(Name = "news")] public int m_newsVersion { get; set; }
    }
}