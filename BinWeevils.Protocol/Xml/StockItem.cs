using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial class StockItemBase
    {
        [XmlField("id")] public uint m_id;
        [XmlField("level")] public uint m_level;
        [XmlField("name")] public string m_name;
        [XmlField("prob")] public int m_probability;
        [XmlField("price")] public uint m_price;
        [XmlField("tyc")] public int m_tycoon;
    }
    
    [XmlCls("item")]
    public partial class FilenameStockItem : StockItemBase
    {
        [XmlField("fileName")] public string m_fileName;
    }
    
    [XmlCls("item")]
    public partial class NestStockItem : FilenameStockItem
    {
        [XmlField("xp")] public uint m_xp;
        [XmlField("clr")] public string m_color;
        [XmlField("descr")] public string m_description;
        [XmlField("dt")] public int m_deliveryTime;
    }
    
    [XmlCls("seed")]
    public partial class SeedStockItem : FilenameStockItem
    {
        [XmlField("cat")] public int m_category;
        [XmlField("mulchYield")] public uint m_mulchYield;
        [XmlField("xpYield")] public uint m_xpYield;
        [XmlField("growTime")] public uint m_growTime;
        [XmlField("cycleTime")] public uint m_cycleTime;
    }
    
    [XmlCls("stock")]
    public partial class Stock
    {
        [XmlBody] public List<NestStockItem> m_items;
        [XmlBody] public List<SeedStockItem> m_seeds;
    }
}