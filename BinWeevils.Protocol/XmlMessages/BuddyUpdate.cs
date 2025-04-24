using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class BuddyUpdateNotification : MsgBody
    {
        [XmlBody] public BuddyUpdateRecord m_record;
    }
    
    [XmlCls("b")]
    public partial class BuddyUpdateRecord
    {
        [XmlField("s")] public bool m_isOnline;
        [XmlField("i")] public int m_userID;
        [XmlField("x")] public bool m_isBlocked;
        
        [XmlBody("n")] public string m_name;
        [XmlBody("vs")] public BuddyVarList m_varList;
    }
    
    [XmlCls("vs")]
    public partial class BuddyVarList
    {
        [XmlBody] private List<BuddyVar> m_buddyVars;
    }
    
    [XmlCls("v")]
    public partial class BuddyVar
    {
        [XmlField("n")] public string m_name;
        [XmlBody] public string m_value;
    }
}