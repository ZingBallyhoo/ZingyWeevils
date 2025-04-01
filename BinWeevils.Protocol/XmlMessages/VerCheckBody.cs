using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class VerCheckBody : MsgBody
    {
        [XmlBody] public Ver m_ver;
        
        [XmlCls("ver")]
        public partial class Ver
        {
            [XmlField("v")] public uint m_ver;
        }
    }
}