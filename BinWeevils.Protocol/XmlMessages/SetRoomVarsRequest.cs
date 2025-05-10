using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class SetRoomVarsRequest : MsgBody
    {
        [XmlBody] public VarList m_varList;
        
        [XmlCls("vars")]
        public partial class VarList
        {
            [XmlBody] public List<Var> m_roomVars;
        }
    
        [XmlCls("var")]
        public partial record struct Var
        {
            [XmlField("n")] public string m_name;
            [XmlField("t")] public string m_type;
            [XmlField("pr")] public bool m_private;
            [XmlField("pe")] public bool m_persistent;
            [XmlBody] public string m_value;
        }
    }
}