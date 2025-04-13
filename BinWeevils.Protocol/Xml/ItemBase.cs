using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record ItemBase
    {
        [XmlField("id")] public uint m_databaseID;
        [XmlField("cat")] public int m_category;
        [XmlField("pc")] public int m_powerConsumption;
    }
}