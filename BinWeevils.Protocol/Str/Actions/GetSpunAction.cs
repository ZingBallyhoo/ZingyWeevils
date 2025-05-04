using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct GetSpunAction
    {
        [StrField] public byte m_vr;
    }
}