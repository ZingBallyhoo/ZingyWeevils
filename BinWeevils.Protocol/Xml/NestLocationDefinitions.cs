using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("nestLocDefs")]
    public partial class NestLocationDefinitions
    {
        [XmlField("version")] public string m_version;

        [XmlBody] public List<NestLocationDefinition> m_locations;
    }
    
    [XmlCls("location")]
    public partial class NestLocationDefinition : LocationDefinition
    {
        [XmlField("cat")] public int m_category;
    }
}