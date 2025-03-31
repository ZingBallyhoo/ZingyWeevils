using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("config")]
    public partial class SiteConfig
    {
        [XmlBody("domain")] public string m_domain;
        [XmlBody("allowMultipleLogins")] public string m_allowMultipleLogins;
        [XmlBody("servicesLocation")] public string m_servicesLocation;
        [XmlBody("restrictFlashPlayers")] public string m_restrictFlashPlayers;
        
        [XmlBody("basePathSmall")] public string m_basePathSmall;
        [XmlBody("basePathLarge")] public string m_basePathLarge;
        [XmlBody("pathItemConfigs")] public string m_pathItemConfigs;
        [XmlBody("pathAssetsNest")] public string m_pathAssetsNest;
        [XmlBody("pathAssetsTycoon")] public string m_pathAssetsTycoon;
        [XmlBody("pathAssetsGarden")] public string m_pathAssetsGarden;
        [XmlBody("pathAssets3D")] public string m_pathAssets3D;
    }
}