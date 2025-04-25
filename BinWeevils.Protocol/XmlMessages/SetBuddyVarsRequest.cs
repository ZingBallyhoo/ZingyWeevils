using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class SetBuddyVarsRequest : MsgBody
    {
        [XmlBody] public VarList m_varList;
        
        // yes, the class names are different between here and update...
        [XmlCls("vars")]
        public partial class VarList
        {
            [XmlBody] public List<Var> m_buddyVars;
        }
    
        [XmlCls("var")]
        public partial class Var
        {
            [XmlField("n")] public string m_name;
            [XmlBody] public string m_value;
        }
    }
}