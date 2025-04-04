using StackXML.Str;

namespace BinWeevils.Protocol.Str.Events
{
    public partial record struct DinerNewChefEvent
    {
        [StrField] public string m_weevilName;
    }
}