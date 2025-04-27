using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("storedGardenItems")]
    public partial class StoredGardenItems
    {
        [XmlBody("storedItem")] public List<GardenInventoryItem> m_items;
        [XmlBody("seed")] public List<NestInventoryItem> m_seeds; // todo
    }
}