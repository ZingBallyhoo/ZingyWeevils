using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("user")]
    public partial struct UserRecord
    {
        [XmlField("id")] public int m_id;
    }
}