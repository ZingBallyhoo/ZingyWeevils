using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class ModMsgBody : MsgBody
    {
        [XmlBody("txt")] public string m_text;
    }
}