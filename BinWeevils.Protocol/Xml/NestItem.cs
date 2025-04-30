using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record NestItemBase : ItemBase
    {
        [XmlField("configName")] public string m_configName;
        [XmlField("clr")] public string m_clrTemp;
    }
    
    [XmlCls("item")]
    public partial record NestInventoryItem : NestItemBase
    {
        [XmlField("dt")] public int m_deliveryTime; // seconds until available...
    }
    
    [XmlCls("item")]
    public partial record NestItem : NestItemBase
    {
        [XmlField("locID")] public uint m_locID;
        [XmlField("crntPos")] public byte m_currentPos;
        [XmlField("fID")] public uint m_placedOnFurnitureID;
        [XmlField("spot")] public byte m_spot;
    }
}