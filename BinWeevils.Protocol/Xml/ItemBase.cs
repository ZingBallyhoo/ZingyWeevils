using System.Xml.Serialization;
using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record ItemBase
    {
        [XmlAttribute("id")] public uint m_databaseID;
        [XmlAttribute("cat")] public int m_category;
        [XmlAttribute("pc")] public int m_powerConsumption;
    }
}