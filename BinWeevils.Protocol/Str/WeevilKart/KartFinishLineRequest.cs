using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct KartFinishLineRequest
    {
        [StrField] public uint m_time;
    }
}