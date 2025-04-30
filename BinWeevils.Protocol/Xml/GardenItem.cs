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
        [XmlField("x")] public short m_x;
        [XmlField("z")] public short m_z;
        [XmlField("r")] public int m_r;
    }
    
    [XmlCls("seed")]
    public partial record GardenInventorySeed 
    {
        [XmlField("id")] public uint m_databaseID;
        [XmlField("name")] public string m_name;
        [XmlField("cat")] public uint m_category;
        [XmlField("fName")] public string m_fileName;
        [XmlField("growTime")] public uint m_growTime;
        [XmlField("cycleTime")] public uint m_cycleTime;
        [XmlField("r")] public uint m_radius;
        [XmlField("mulch")] public uint m_mulch;
        [XmlField("xp")] public uint m_xp;
    }
    
    [XmlCls("plant")]
    public partial record GardenPlant : GardenInventorySeed
    {
        [XmlField("age")] public uint m_age;
        [XmlField("watered")] public bool m_watered;
        [XmlField("x")] public short m_x;
        [XmlField("z")] public short m_z;
    }
}