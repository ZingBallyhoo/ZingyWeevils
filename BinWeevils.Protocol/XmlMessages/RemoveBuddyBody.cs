using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class RemoveBuddyBody : MsgBody
    {
        [XmlBody("n")] public string m_buddyName;
    }
}