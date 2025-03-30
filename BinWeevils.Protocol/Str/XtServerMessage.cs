using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public ref partial struct XtServerMessage
    {
        [StrField] public ReadOnlySpan<char> m_command;
        [StrField] public int m_roomID;
    }
}