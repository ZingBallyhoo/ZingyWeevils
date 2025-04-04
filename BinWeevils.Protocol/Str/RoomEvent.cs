using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct ClientRoomEvent
    {
        [StrField] public string m_a;
        [StrField] public string m_b;
    }
    
    public partial record struct ServerRoomEvent
    {
        [StrField] public string m_a;
    }
}