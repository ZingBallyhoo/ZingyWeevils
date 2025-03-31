using System.Diagnostics.CodeAnalysis;
using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public record CheckVersionResponse
    {
        [FormUrlEncodedPropertyName("vOK")] public int m_ok { get; set; }
        [FormUrlEncodedPropertyName("core")] public int m_coreVersionNumber { get; set; }
        [FormUrlEncodedPropertyName("VODplayer")] public int m_vodPlayerVersion { get; set; }
        [FormUrlEncodedPropertyName("VODcontent")] public int m_vodContentVersion { get; set; }
        [FormUrlEncodedPropertyName("locDef")] public int m_locDefVersion { get; set; }
        [FormUrlEncodedPropertyName("nestDef")] public int m_nestDefVersion { get; set; }
        [FormUrlEncodedPropertyName("p")] public int m_p { get; set; }
        [FormUrlEncodedPropertyName("binBadgesDisplay")] public int m_binBadgesDisplayVersion { get; set; }
        [FormUrlEncodedPropertyName("achievementAlerts")] public int m_achievementAlertsVersion { get; set; }
        [FormUrlEncodedPropertyName("URLDef")] public int m_urlDefVersion { get; set; }
        [FormUrlEncodedPropertyName("news")] public int m_newsVersion { get; set; }
    }
}