using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial record struct HoldTrayAction
    {
        [StrField] public int m_trayID;
    }
}