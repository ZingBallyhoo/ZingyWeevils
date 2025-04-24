using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class BuddyRoomResponse : MsgBody
    {
        [XmlBody] public BuddyRoomList m_list;
    }

    [XmlCls("br")]
    public partial class BuddyRoomList
    {
        [XmlField("r")] [XmlSplitStr(',')] public List<ulong> m_rooms;
    }
}