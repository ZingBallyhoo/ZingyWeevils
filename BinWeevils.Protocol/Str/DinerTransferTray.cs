using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct DinerTransferTray
    {
        [StrField] public int m_trayId;
    }
}