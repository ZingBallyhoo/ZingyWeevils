using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public ref partial struct XtClientMessage
    {
        [StrField] public ReadOnlySpan<char> m_extension;
        [StrField] public ReadOnlySpan<char> m_command;
        [StrField] public int m_roomID;
    }
}