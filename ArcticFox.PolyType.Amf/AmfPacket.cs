using PolyType;

namespace ArcticFox.PolyType.Amf
{
    [GenerateShape]
    public partial class AmfPacket
    {
        public AmfVersion m_version = AmfVersion.Three;
        public List<AmfHeader> m_headers = new List<AmfHeader>();
        public List<AmfMessage> m_messages = new List<AmfMessage>();
    }
    
    [GenerateShape]
    public partial class AmfHeader
    {
        public string m_name;
        public bool m_mustUnderstand;
        public object? m_content;
    }
    
    [GenerateShape]
    public partial class AmfMessage
    {
        public string m_targetUri;
        public string m_responseUri;
        public object? m_data;
    }
}