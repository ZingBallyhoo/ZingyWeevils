using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class UserLeaveRoomBody : MsgBody
    {
        [XmlBody] public UserRecord m_user;
    }
}