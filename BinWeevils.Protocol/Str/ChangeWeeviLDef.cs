using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public ref partial struct ChangeWeevilDef
    {
        [StrField] public ReadOnlySpan<char> m_weevilDef;
    }
}