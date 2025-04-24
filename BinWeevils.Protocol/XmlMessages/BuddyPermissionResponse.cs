using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class BuddyPermissionResponse : MsgBody
    {
        [XmlBody] public Inner m_inner;

        [XmlCls("n")]
        public partial class Inner
        {
            [XmlField("res")] public string m_result;
            [XmlBody] public string m_name;
        }
    }
}