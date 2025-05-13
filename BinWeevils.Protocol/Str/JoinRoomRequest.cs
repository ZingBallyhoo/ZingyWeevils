using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial class JoinRoomRequest
    {
        [StrField] public string m_roomName;
        [StrField] public double m_entryX;
        [StrField] public double m_entryY;
        [StrField] public double m_entryZ;
        [StrField] public int m_entryDir;
        [StrField] public byte m_entryDoorID;
        [StrField] public int m_locID;
    }
}