using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public record GamePlayer(User user);
    
    public class TurnBasedGame : IRoomEventHandler
    {
        private readonly Room m_room;
        
        public TurnBasedGame(Room room)
        {
            m_room = room;
        }
        
        public async ValueTask IncomingRequest(User user, TurnBasedGameRequest request)
        {
            // authenticate
            switch (request.m_command)
            {
                case Modules.TURN_BASED_JOIN:
                {
                    await user.MoveTo(m_room);
                    // todo: try join or spec...
                    return;
                }
                case Modules.TURN_BASED_REMOVE_PLAYER:
                {
                    await user.RemoveFromRoom(BinWeevilsSocketHost.TURN_BASED_GAME_ROOM_TYPE);
                    return;
                }
            }
            
            var usersGameRoom = await user.GetRoom(BinWeevilsSocketHost.TURN_BASED_GAME_ROOM_TYPE);
            if (!ReferenceEquals(usersGameRoom, m_room))
            {
                throw new InvalidDataException("user sent a request to a game they aren't in");
            }
            
            Console.Out.WriteLine(request);
        }

        public ValueTask UserLeftRoom(Room room, User user)
        {
            // todo: broadcast if player
            return ValueTask.CompletedTask;
        }
    }
    
    public class Mulch4Game : TurnBasedGame
    {
        public Mulch4Game(Room room) : base(room)
        {
        }
    }
}