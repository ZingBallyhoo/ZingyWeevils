using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class XtResBody : MsgBody
    {
        [XmlBody] public string m_xmlBody;
    }

    [XmlCls("dataObj")]
    public partial class ActionScriptObject
    {
        [XmlBody] public List<Var> m_vars;
        [XmlBody] public List<SubActionScriptObject> m_objects;
    }

    [XmlCls("obj")]
    public partial class SubActionScriptObject : ActionScriptObject
    {
        [XmlField("o")] public string m_name;
        [XmlField("t")] public string m_type;
    }
}