using System.Diagnostics;
using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("msg")]
    public partial struct Msg
    {
        [XmlField("t")] public string m_messageType = "sys";
        [XmlBody] public MsgBody m_body;
    }

    [XmlCls("body")]
    public partial class MsgBody
    {
        [XmlField("action")] public string m_action;
        [XmlField("r")] public int m_room = -1;
    }

    [XmlCls("pid")]
    [DebuggerDisplay("MsgBodyPID: {m_id}")]
    public partial struct MsgBodyPID
    {
        [XmlField("id")] public int m_id;
    }
}