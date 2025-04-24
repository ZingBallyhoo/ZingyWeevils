using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class BuddyListResponse : MsgBody
    {
        [XmlBody] public BuddyList m_list;
    }

    [XmlCls("bList")]
    public partial class BuddyList
    {
        [XmlBody] public List<BuddyUpdateRecord> m_buddies;
    }
}