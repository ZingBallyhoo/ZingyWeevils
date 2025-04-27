using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record GardenItemBase : ItemBase
    {
        [XmlField("fName")] public string m_fileName;
    }
    
    [XmlCls("storedItem")]
    public partial record GardenInventoryItem : GardenItemBase
    {
        [XmlField("dt")] public int m_deliveryTime; // seconds until available...
    }

    [XmlCls("item")]
    public partial record GardenItem : GardenItemBase
    {
        [XmlField("locID")] public uint m_locID;
        [XmlField("x")] public int m_x;
        [XmlField("z")] public int m_z;
        [XmlField("r")] public int m_r;
    }
}