using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public ref partial struct KartRequestHeader
    {
        [StrField] public ReadOnlySpan<char> m_command;
    }
}