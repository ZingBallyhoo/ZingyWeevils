using StackXML.Str;

namespace BinWeevils.Protocol.Str.Events
{
    public partial record struct DinerTransferTrayEvent
    {
        [StrField] public int m_trayId;
        [StrField] public string m_weevilName;
    }
}