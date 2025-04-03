using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct TeleportOutAction
    {
        [StrField] public int m_destLocID;
    }
}