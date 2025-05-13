using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial struct ClientExpression
    {
        [StrField] public byte m_expressionID;
    }
    
    public partial struct ServerExpression
    {
        [StrField] public ulong m_uid;
        [StrField] public byte m_expressionID;
    }
}