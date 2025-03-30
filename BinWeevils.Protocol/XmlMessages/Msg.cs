using System.Diagnostics;
using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("msg")]
    public partial class Msg
    {
        [XmlField("t")] public string m_messageType;
        [XmlBody] public MsgBody m_body;
    }

    [XmlCls("body")]
    public partial class MsgBody
    {
        [XmlField("action")] public string m_action;
        [XmlField("r")] public int m_room;
    }

    [XmlCls("pid")]
    [DebuggerDisplay("MsgBodyPID: {m_id}")]
    public partial class MsgBodyPID
    {
        [XmlField("id")] public int m_id;
    }
}