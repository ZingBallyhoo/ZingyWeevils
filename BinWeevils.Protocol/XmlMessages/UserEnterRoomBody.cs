using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class UserEnterRoomBody : MsgBody
    {
        [XmlBody] public UserJoinRecord m_user;
    }

    [XmlCls("u")]
    public partial struct UserJoinRecord
    {
        [XmlField("i")] public ulong m_uid;
        [XmlField("m")] public bool m_isModerator;
        [XmlBody("n")] public string m_name;
        [XmlBody] public VarList m_vars;
    }
}