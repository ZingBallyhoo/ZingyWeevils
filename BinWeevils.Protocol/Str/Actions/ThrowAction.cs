using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct ThrowAction
    {
        [StrField] public uint m_petID;
        [StrField] public byte m_ballID;
        [StrField] public double m_xTarg;
        [StrField] public double m_zTarg;
    }
}