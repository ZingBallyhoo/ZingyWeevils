using ArcticFox.SmartFoxServer;

namespace BinWeevils.GameServer
{
    public static class WeevilRoomExtensions
    {
        public static WeevilRoomDescription GetWeevilDesc(this Room room)
        {
            return (WeevilRoomDescription)room.m_description;
        }
        
        public static bool IsLimbo(this Room room)
        {
            return room.GetWeevilDesc().m_limbo;
        }
    }
}