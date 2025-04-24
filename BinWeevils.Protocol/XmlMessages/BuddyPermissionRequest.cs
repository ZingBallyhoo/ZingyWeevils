using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class BuddyPermissionRequest : MsgBody
    {
        [XmlBody("n")] public string m_sender;
        [XmlBody("txt")] public string m_message = "";
    }
}