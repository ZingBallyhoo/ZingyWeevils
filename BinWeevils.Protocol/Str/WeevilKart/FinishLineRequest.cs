using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record FinishLineRequest
    {
        [StrField] public uint m_time;
    }
}