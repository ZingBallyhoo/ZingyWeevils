using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("itemConfig")]
    public partial class ItemConfig
    {
        [XmlField("id")] public string m_id;
        [XmlField("type")] public string m_type;
        [XmlField("path")] public string m_path;
        [XmlField("thumb")] public string m_thumbPath;
        
        [XmlField("useCache")] public bool m_useCache;
        [XmlField("noSell")] public bool m_noSell;
        
        [XmlField("h")] public int m_height;
        [XmlField("boundType")] public string m_boundType;
        [XmlField("boundRadius")] public float m_boundRadius; // for "radial"
        
        [XmlField("numSpots")] public byte m_numSpots;
        [XmlField("clickable")] public bool m_clickable;
        
        [XmlBody] public List<ItemPos> m_positions;
    }
    
    [XmlCls("pos")]
    public partial class ItemPos
    {
        [XmlField("frame")] public byte m_frame;
        [XmlField("bounds")] public string m_bounds;
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
        [XmlField("gridSqs")] public string m_gridSqs;
    }
}