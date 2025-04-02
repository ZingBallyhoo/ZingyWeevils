using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial struct ClientExpression
    {
        [StrField] public int m_expressionID;
    }
    
    public partial struct ServerExpression
    {
        [StrField] public int m_uid;
        [StrField] public int m_expressionID;
    }
}