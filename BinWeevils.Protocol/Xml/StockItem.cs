using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial class StockItem
    {
        [XmlField("id")] public uint m_id;
        [XmlField("level")] public uint m_level;
        [XmlField("clr")] public string m_color;
        [XmlField("name")] public string m_name;
        [XmlField("prob")] public int m_probability;
        [XmlField("price")] public uint m_price;
        [XmlField("descr")] public string m_description;
        
        // anything but hats:
        [XmlField("tyc")] public int m_tycoon;
        [XmlField("xp")] public uint m_xp;
        [XmlField("fileName")] public string m_fileName;
        [XmlField("dt")] public int m_deliveryTime;
    }
    
    [XmlCls("seed")]
    public partial class SeedStockItem : StockItem
    {
        // todo: cat
        // todo: mulchYield
        // todo: xpYield
        // todo: growTime
        // todo: cycleTime
    }
    
    [XmlCls("stock")]
    public partial class Stock
    {
        [XmlBody] public List<StockItem> m_items;
        [XmlBody] public List<SeedStockItem> m_seeds;
    }
}