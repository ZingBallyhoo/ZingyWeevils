using System.Xml.Serialization;
using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("item")]
    public partial record GardenItemBase : ItemBase
    {
        [XmlAttribute("fName")] public string m_fileName;
    }

    [XmlCls("item")]
    public partial record GardenItem : GardenItemBase
    {
        [XmlAttribute("locID")] public uint m_locID;
        [XmlAttribute("x")] public int m_x;
        [XmlAttribute("z")] public int m_z;
        [XmlAttribute("r")] public int m_r;
    }
}