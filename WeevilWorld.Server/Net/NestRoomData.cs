using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public class NestRoomData
    {
        public readonly WeevilData m_owner;
        public readonly long m_slot;
        private Room? m_room;

        public bool m_lightOn = true;
        public bool m_purchased;

        public NestRoomData(WeevilData owner, long slot)
        {
            m_owner = owner;
            m_slot = slot;
        }

        public async ValueTask SetOwned()
        {
            if (m_purchased) throw new InvalidDataException("already owned");
            var owningUser = m_owner.m_user;
            m_room = await owningUser.m_zone.CreateRoom(new RoomDescription
            {
                m_creator = owningUser,
                m_maxUsers = 20,
                m_isTemporary = true,
                m_name = WeevilWorldSocketHost.GetNestRoomName(m_owner.m_object, m_slot)
            });
            m_room.SetData(this);
            m_purchased = true;
        }

        public Room Room()
        {
            if (!m_purchased) throw new InvalidDataException("not owned");
            Debug.Assert(m_room != null);
            return m_room;
        }

        public async ValueTask ToggleLights(User sender)
        {
            m_lightOn = !m_lightOn;

            var broadcaster = new FilterBroadcaster<User>(Room().m_userExcludeFilter, sender);
            await broadcaster.Broadcast(PacketIDs.LIGHTSTATECHANGED_NOTIFICATION, new LightStateChanged
            {
                Idx = m_owner.m_object.Idx,
                Slot = m_slot,
                LightOn = m_lightOn
            });

            await sender.Broadcast(PacketIDs.TOGGLELIGHT_RESPONSE, new ToggleLight
            {
                Std = new StdResponse
                {
                    Result = ResultType.Ok
                },
                LightOn = m_lightOn,
                Success = true
            });
        }
    }
}