using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    /*[XmlCls("msg")]
    public ref partial struct MsgPreRead
    {
        [XmlField("t")] public string m_messageType;
        [XmlBody("body")] public MsgBodyPreRead m_body;
    }
    
    [XmlCls("body")]
    public ref partial struct MsgBodyPreRead
    {
        [XmlField("action")] public string m_action;
        [XmlField("r")] public int m_room;
    }*/
    
    public ref struct MsgPreRead : IXmlSerializable
    {
        [XmlField("t")] public string m_messageType;
        public ReadOnlySpan<char> m_bodySpan;
        
        public ReadOnlySpan<char> GetNodeName()
        {
            return "msg";
        }

        public bool ParseAttribute(ref XmlReadBuffer buffer, ReadOnlySpan<char> name, ReadOnlySpan<char> value)
        {
            switch (name)
            {
                case "t": {
                    this.m_messageType = value.ToString();
                    return true;
                }
            }
            return false;
        }

        public void SerializeAttributes(ref XmlWriteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public bool ParseSubBody(ref XmlReadBuffer buffer, ReadOnlySpan<char> name, ReadOnlySpan<char> bodySpan, ReadOnlySpan<char> innerBodySpan, ref int end, ref int endInner)
        {
            switch (name)
            {
                case "body": {
                    m_bodySpan = bodySpan;
                    buffer.m_abort = true;
                    return true;
                }
            }
            return false;
        }
        public bool ParseFullBody(ref XmlReadBuffer buffer, ReadOnlySpan<char> bodySpan, ref int end)
        {
            return false;
        }

        public void SerializeBody(ref XmlWriteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
    
    public ref struct MsgBodyPreRead : IXmlSerializable
    {
        [XmlField("action")] public ReadOnlySpan<char> m_action;
        [XmlField("r")] public int m_room;
        
        public ReadOnlySpan<char> GetNodeName()
        {
            return "body";
        }

        public bool ParseAttribute(ref XmlReadBuffer buffer, ReadOnlySpan<char> name, ReadOnlySpan<char> value)
        {
            switch (name)
            {
                case "r": {
                    this.m_room = buffer.m_params.m_stringParser.Parse<System.Int32>(value);
                    return true;
                }
                case "action": {
                    this.m_action = value;
                    return true;
                }
            }
            return false;
        }

        public void SerializeAttributes(ref XmlWriteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public bool ParseFullBody(ref XmlReadBuffer buffer, ReadOnlySpan<char> bodySpan, ref int end)
        {
            buffer.m_abort = true;
            return true;
        }
        
        public bool ParseSubBody(ref XmlReadBuffer buffer, ReadOnlySpan<char> name, ReadOnlySpan<char> bodySpan, ReadOnlySpan<char> innerBodySpan, ref int end, ref int endInner)
        {
            throw new NotImplementedException();
        }

        public void SerializeBody(ref XmlWriteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}