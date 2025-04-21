using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public ref partial struct ClientRequest
    {
        [StrField] public ReadOnlySpan<char> m_command;
    }
}