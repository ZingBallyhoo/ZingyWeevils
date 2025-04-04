using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class PubMsgBody : MsgBody
    {
        [XmlBody("txt")] public string m_text;
    }
    
    [XmlCls("body")]
    public partial class ServerPubMsgBody : PubMsgBody
    {
        [XmlBody] public UserRecord m_user;
    }
}