using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("plantConfigs")]
    public partial class PlantConfigs
    {
        [XmlField("weevilHappiness")] public byte m_weevilHappiness;
        [XmlBody] public List<GardenPlant> m_plants;
    }
}