using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("allStoredItems")]
    public partial class StoredHaggleItems
    {
        [XmlBody] public List<HaggleItem> m_items;
    }
    
    [XmlCls("item")]
    public partial class HaggleItem
    {
        [XmlField("id")] public uint m_databaseID;
        [XmlField("type")] public byte m_type;
        [XmlField("clr")] public ItemColor m_color;
        [XmlField("configName")] public string m_configLocation;
        [XmlField("value")] public uint m_value;
    }
    
    public enum EHaggleItemType
    {
        NestItem = 70,
        GardenItem = 71,
        Seed = 72,
    }
    
    public enum EHaggleSaleType
    {
        Default = 1, // 20%
        GambleLow = 2, // 10%
        GambleOkay = 3, // 15%
        GambleBest = 4, // 35%
    }
}