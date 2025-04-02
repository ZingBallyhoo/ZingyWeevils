using System.Diagnostics;
using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class JoinRoomResponse  : MsgBody
    {
        [XmlBody] public MsgBodyPID m_pid;
        [XmlBody] public VarList m_vars = new VarList();
        [XmlBody] public RoomPlayerList m_playerList = new RoomPlayerList();
    }

    [XmlCls("uLs")]
    public partial struct RoomPlayerList
    {
        [XmlField("r")] public int m_room;
        [XmlBody] public List<RoomPlayer> m_players = new List<RoomPlayer>();
    }
    
    [XmlCls("u")]
    [DebuggerDisplay("RoomPlayer: {m_name}")]
    public partial struct RoomPlayer
    {
        [XmlField("i")] public int m_uid;
        [XmlField("m")] public int m_moderator;
        
        [XmlBody("n")] public string m_name;
        [XmlBody] public VarList m_vars = new VarList();
    }
}