using ArcticFox.SmartFoxServer;

namespace BinWeevils.GameServer
{
    public record WeevilRoomDescription(string m_name) : RoomDescription(m_name)
    {
        public bool m_limbo;
        public bool m_isGame;
    }
}