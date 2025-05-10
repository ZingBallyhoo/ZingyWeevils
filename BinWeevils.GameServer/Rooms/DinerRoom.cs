using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.Str.Events;
using BinWeevils.Protocol.XmlMessages;
using StackXML.Str;

namespace BinWeevils.GameServer.Rooms
{
    public class DinerRoom : StatefulRoom<DinerRoomData>, IRoomEventHandler
    {
        public DinerRoom(int plateCount, int trayCount) : base(new DinerRoomData(plateCount, trayCount))
        {
        }

        public override async ValueTask ClientSentRoomEvent(User user, ClientRoomEvent roomEvent)
        {
            var eventReader = new StrReader(roomEvent.m_eventParams, ';', WeevilStrParser.s_instance);
            var eventID = (DinerEventID)eventReader.Get<int>();
            
            // Console.Out.WriteLine($"diner event: {eventID} - {roomEvent.m_eventParams} - {roomEvent.m_roomState}");

            switch (eventID)
            {
                case DinerEventID.SET_FOOD:
                {
                    var setFood = new DinerEventSetFood();
                    setFood.FullyDeserialize(ref eventReader);
                    
                    await TrySetFood(setFood);
                    break;
                }
                case DinerEventID.EAT:
                {
                    var eatFood = new DinerEventEatFood();
                    eatFood.FullyDeserialize(ref eventReader);
                    
                    await TryEatFood(eatFood);
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"client sent unexpected diner event: {eventID}");
                    break;
                }
            }
        }
        
        public async ValueTask<bool> TryStartChef(string userName)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            if (!string.IsNullOrEmpty(data.m_chef.GetValue()))
            {
                // there is already a chef -_-
                return false;
            }
            
            await SetChef(data, userName);
            return true;
        }
        
        public async ValueTask<bool> TryQuitChef(string userName)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            if (!string.Equals(data.m_chef.GetValue(), userName))
            {
                // you can't quit as chef, you aren't one -_-
                return false;
            }
            
            await SetChef(data, null);
            return true;
        }
        
        public async ValueTask TryGrabTray(int id, string userName)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            if (!string.IsNullOrEmpty(data.GetTrayOwner(id)))
            {
                // someone already owns this tray -_-
                return;
            }
            
            await TransferTray(data, id, userName);
        }
        
        public async ValueTask TryDropTray(int id, string userName)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            if (data.GetTrayOwner(id) != userName)
            {
                // someone else owns this tray -_-
                return;
            }
            
            await TransferTray(data, id, null);
        }
        
        private async ValueTask TrySetFood(DinerEventSetFood setFood)
        {
            // (also used to delete food after adding it to a tray)
            
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            ref var plateVar = ref data.GetPlate(setFood.m_plateId);
            if (plateVar.GetValue() != 0 && setFood.m_foodId != 0)
            {
                // plate already has food on it...
                return;
            }
            
            plateVar.SetValue(setFood.m_foodId);
            await m_room.BroadcastRoomEvent((int)DinerEventID.SET_FOOD, setFood);
        }
        
        private async ValueTask TryEatFood(DinerEventEatFood eatFood)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            
            ref var plateVar = ref data.GetPlate(eatFood.m_plateId);
            if (plateVar.GetValue() == 0)
            {
                // plate doesn't have any food on it...
                return;
            }
            
            plateVar.SetValue(0);
            await m_room.BroadcastRoomEvent((int)DinerEventID.EAT, eatFood);
        }
        
        private async ValueTask SetChef(DinerRoomData data, string? weevilName)
        {
            weevilName ??= string.Empty;
            
            data.m_chef.SetValue(weevilName);
            await m_room.BroadcastRoomEvent((int)DinerEventID.NEW_CHEF, new DinerNewChefEvent
            {
                m_weevilName = weevilName
            });
        }
        
        private async ValueTask TransferTray(DinerRoomData data, int trayId, string? weevilName)
        {
            var weevilNameForVar = weevilName ?? string.Empty;
            var weevilNameForEvent = weevilName ?? "0";
            // client only understands "0" as nobody for event... parsing on join is fine
            
            data.GetTrayOwner(trayId).SetValue(weevilNameForVar);
            await m_room.BroadcastRoomEvent((int)DinerEventID.TRANSFER_TRAY, new DinerTransferTrayEvent
            {
                m_trayId = trayId,
                m_weevilName = weevilNameForEvent
            });
        }
        
        public async ValueTask UserLeftRoom(Room room, User user)
        {
            using var dataToken = await m_vars.Get();
            var data = dataToken.m_value;
            if (data.m_chef == user.m_name)
            {
                await SetChef(data, string.Empty);
            }

            for (int i = 1; i < data.m_trayOwners.Length; i++)
            {
                if (data.m_trayOwners[i] != user.m_name) continue;
                await TransferTray(data, i, null);
            }
        }
    }
    
    public class DinerRoomData : VarBag
    {
        public TypedVar<string> m_chef;
        private readonly TypedVar<int>[] m_plates;
        public readonly TypedVar<string>[] m_trayOwners;
        
        public DinerRoomData(int plateCount, int trayCount)
        {
            m_chef = new TypedVar<string>(this, "c", Var.TYPE_STRING);
            
            // trays at the bar are also plates...
            plateCount += trayCount;
            
            m_plates = new TypedVar<int>[plateCount+1];
            for (var i = 0; i < plateCount+1; i++)
            {
                m_plates[i] = new TypedVar<int>(this, $"p{i}", Var.TYPE_NUMBER);
            }
            
            m_trayOwners = new TypedVar<string>[trayCount+1];
            for (var i = 1; i < trayCount+1; i++)
            {
                m_trayOwners[i] = new TypedVar<string>(this, $"t{i}", Var.TYPE_STRING);
            }
        }
        
        public ref TypedVar<int> GetPlate(int id)
        {
            if (id == 0) throw new InvalidDataException();
            return ref m_plates[id];
        }
        
        public ref TypedVar<string> GetTrayOwner(int id)
        {
            if (id == 0) throw new InvalidDataException();
            return ref m_trayOwners[id];
        }
    }
}