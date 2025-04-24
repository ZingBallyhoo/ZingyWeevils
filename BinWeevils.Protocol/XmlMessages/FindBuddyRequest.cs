using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class FindBuddyRequest : MsgBody
    {
        [XmlBody] public Inner m_record;
        
        [XmlCls("b")]
        public partial class Inner
        {
            [XmlField("id")] public ulong m_id;
        }
    }
}