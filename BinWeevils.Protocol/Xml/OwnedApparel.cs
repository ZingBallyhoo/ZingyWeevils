using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("apparelOwned")]
    public partial class OwnedApparelList
    {
        [XmlBody] public List<OwnedApparelEntry> m_items;
    }
    
    [XmlCls("item")]
    public partial record struct OwnedApparelEntry
    {
        [XmlField("id")] public uint m_id;
        [XmlField("cat")] public byte m_category;
        [XmlField("rgb")] public string m_rgb;
        [XmlField("worn")] public bool m_worn;
    }
}