using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record KartFinishLineRequest
    {
        [StrField] public uint m_time;
    }
}