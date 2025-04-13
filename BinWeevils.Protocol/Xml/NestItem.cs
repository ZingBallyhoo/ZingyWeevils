using System.Xml.Serialization;
using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record NestItemBase
    {
        [XmlAttribute("configName")] public string m_configName;
        [XmlAttribute("clr")] public string m_clrTemp;
    }
    
    [XmlCls("item")]
    public partial record NestInventoryItem : NestItemBase
    {
        [XmlAttribute("dt")] public int m_deliveryTime; // seconds until available...
    }
    
    [XmlCls("item")]
    public partial record NestItem : NestItemBase
    {
        [XmlAttribute("locID")] public uint m_locID;
        [XmlAttribute("crntPos")] public uint m_currentPos;
        [XmlField("fID")] public uint m_placedOnFurnitureID;
        [XmlField("spot")] public uint m_spot;
    }
}