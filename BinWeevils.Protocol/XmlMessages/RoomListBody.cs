using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class RoomListBody : MsgBody
    {
        [XmlBody] public RoomList m_roomList;
    }
    
    [XmlCls("rmList")]
    public partial struct RoomList
    {
        [XmlBody] public List<RoomInfo> m_rooms = new List<RoomInfo>();

        public void Add(RoomInfo room)
        {
            m_rooms.Add(room);
        }
    }
    
    [XmlCls("rm")]
    public partial class RoomRecord
    {
        [XmlField("id")] public int m_id;
    }
    
    [XmlCls("rm")]
    public partial class RoomInfo : RoomRecord
    {
        [XmlField("priv")] public bool m_private;
        [XmlField("temp")] public bool m_temp;
        [XmlField("game")] public bool m_game;
        [XmlField("ucnt")] public int m_userCount;
        [XmlField("lmb")] public bool m_limbo;
        [XmlField("scnt")] public int m_spectatorCount;
        [XmlField("maxu")] public int m_maxUsers;
        [XmlField("maxs")] public int m_maxSpectators;

        [XmlBody("n")] public string m_name;
    }
}