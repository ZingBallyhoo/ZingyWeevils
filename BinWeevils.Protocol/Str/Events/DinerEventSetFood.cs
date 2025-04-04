using StackXML.Str;

namespace BinWeevils.Protocol.Str.Events
{
    public partial record struct DinerEventSetFood
    {
        [StrField] public int m_plateId;
        [StrField] public int m_foodId;
    }
}