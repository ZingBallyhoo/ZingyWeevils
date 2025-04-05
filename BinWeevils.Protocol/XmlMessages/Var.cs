using System.Diagnostics;
using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("var")]
    [DebuggerDisplay("Var: {m_name}: {m_value} ({m_type})")]
    public partial struct Var
    {
        [XmlField("n")] public string m_name;
        [XmlField("t")] public string m_type;
        [XmlBody] public string m_value;
        
        public const string TYPE_NULL = "x";
        public const string TYPE_BOOLEAN = "b";
        public const string TYPE_NUMBER = "n";
        public const string TYPE_STRING = "s";

        public Var()
        {
        }

        public Var(string name, string type, string value)
        {
            m_name = name;
            m_type = type;
            m_value = value;
        }

        public static Var Null(string name)
        {
            return new Var(name, TYPE_NULL, string.Empty);
        }
        
        public static Var Number(string name, int value)
        {
            return new Var(name, TYPE_NUMBER, $"{value}");
        }
        
        public static Var String(string name, string value)
        {
            return new Var(name, TYPE_STRING, value);
        }
    }
    
    [XmlCls("vars")]
    public partial struct VarList
    {
        [XmlBody] public List<Var> m_vars;
    }
}