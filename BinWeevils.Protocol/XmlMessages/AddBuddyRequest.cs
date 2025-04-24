using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class AddBuddyRequest : MsgBody
    {
        [XmlBody("n")] public string m_targetName;
    }
}