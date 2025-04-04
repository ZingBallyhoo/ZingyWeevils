using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct ServerChatState
    {
        [StrField] public int m_state;
    }
}