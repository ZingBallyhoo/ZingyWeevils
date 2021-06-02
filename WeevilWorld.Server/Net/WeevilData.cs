using System.Collections.Generic;
using ArcticFox.SmartFoxServer;
using WeevilWorldProtobuf.Objects;

namespace WeevilWorld.Server.Net
{
    public class WeevilData
    {
        public readonly User m_user;
        public readonly Weevil m_object;
        private readonly NestRoomData[] m_nestRooms;

        public ulong? m_currentlyInvitedUser { private get; set; }

        public WeevilData(User user, Weevil weevil)
        {
            m_user = user;
            m_object = weevil;
                
            m_nestRooms = new NestRoomData[10]; // todo: idk how many
            for (var i = 0; i < m_nestRooms.Length; i++)
            {
                m_nestRooms[i] = new NestRoomData(this, i+1);
            }
        }

        public ulong? GetInvitedPlayer()
        {
            var current = m_currentlyInvitedUser;
            if (current == null || current.Value == 0) return null;
            return current.Value;
        }

        public bool IsRoomInvited()
        {
            var current = m_currentlyInvitedUser;
            return current == 0;
        }

        public void RemoveInvite()
        {
            m_currentlyInvitedUser = null;
        }

        public NestRoomData GetNestRoom(long slot)
        {
            return m_nestRooms[slot - 1];
        }

        public IReadOnlyList<NestRoomData> GetRooms()
        {
            return m_nestRooms;
        }
    }
}