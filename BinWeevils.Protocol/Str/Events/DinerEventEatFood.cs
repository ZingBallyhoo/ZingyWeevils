using StackXML.Str;

namespace BinWeevils.Protocol.Str.Events
{
    public partial record struct DinerEventEatFood
    {
        [StrField] public int m_plateId;
    }
}