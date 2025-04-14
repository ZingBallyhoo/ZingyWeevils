using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("nestConfig")]
    public partial class NestConfig
    {
        [XmlField("id")] public uint m_id;
        [XmlField("idx")] public uint m_idx;
        [XmlField("lastUpdate")] public string m_lastUpdate;
        [XmlField("score")] public uint m_score;
        [XmlField("gardenCanSubmit")] public bool m_gardenCanSubmit;
        [XmlField("fuel")] public uint m_fuel;
        [XmlField("weevilXp")] public uint m_weevilXp;
        [XmlField("gardenSize")] public uint m_gardenSize;
        
        [XmlBody] public List<Loc> m_locs;
        [XmlBody] public List<NestItem> m_items;
        
        [XmlCls("loc")]
        public partial class Loc
        {
            [XmlField("id")] public uint m_id;
            [XmlField("instanceID")] public uint m_instanceID;
            [XmlField("colour")] public NestRoomColor m_color;
        }
    }
}