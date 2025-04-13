using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("storedItems")]
    public partial class StoredItems
    {
        [XmlBody] public List<NestInventoryItem> m_items;
    }
}