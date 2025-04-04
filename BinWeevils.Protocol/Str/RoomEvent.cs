using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct ClientRoomEvent
    {
        [StrField] public string m_eventParams;
        [StrField] public string m_roomState;
    }
    
    public partial record struct ServerRoomEvent
    {
        [StrField] public string m_eventParams;
    }
}