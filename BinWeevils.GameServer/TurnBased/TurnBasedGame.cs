using ArcticFox.Net.Util;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public abstract class TurnBasedGame
    {
        public abstract ValueTask IncomingRequest(User user, TurnBasedGameRequest request);
    }
    
    public abstract class TurnBasedGame<T> : TurnBasedGame, IRoomEventHandler where T : TurnBasedGameData, new()
    {
        protected readonly Room m_room;
        private readonly AsyncLockedAccess<T> m_gameData;
        private bool m_recycling;

        protected TurnBasedGame(Room room)
        {
            m_room = room;
            m_gameData = new AsyncLockedAccess<T>(new T());
            m_recycling = false;
        }
        
        protected async ValueTask<AsyncLockedAccess<T>.Token> GetData()
        {
            return await m_gameData.Get();
        }
        
        public override async ValueTask IncomingRequest(User user, TurnBasedGameRequest request)
        {
            if (user.m_name != request.m_userID)
            {
                throw new InvalidDataException("user sent a request for somebody else");
            }
            
            switch (request.m_command)
            {
                case Modules.TURN_BASED_JOIN:
                {
                    await JoinGame(user, request);
                    return;
                }
                case Modules.TURN_BASED_REMOVE_PLAYER:
                {
                    // note: the client calls this even if it never joined
                    await user.RemoveFromRoom(BinWeevilsSocketHost.TURN_BASED_GAME_ROOM_TYPE);
                    return;
                }
            }
            
            var usersGameRoom = await user.GetRoomOrNull(BinWeevilsSocketHost.TURN_BASED_GAME_ROOM_TYPE);
            if (usersGameRoom == null)
            {
                // sometimes when the game gets cancelled (stalemate, quit) the client is still able to send turns
                return;
            }
            
            if (!ReferenceEquals(usersGameRoom, m_room))
            {
                throw new InvalidDataException("user sent a request to a game they aren't in");
            }
            
            Console.Out.WriteLine(request);

            switch (request.m_command)
            {
                case Modules.TURN_BASED_TAKE_TURN:
                {
                    await HandleTurnRequest(request);
                    break;
                }
                case Modules.TURN_BASED_PLAYER_WINS:
                {
                    await HandlePlayerWinsRequest(request);
                    break;
                }
                default:
                {
                    throw new Exception($"unknown request: {request}");
                    break;
                }
            }
        }

        protected TResponse MakeResponse<TResponse>(TurnBasedGameRequest request, T data) where TResponse : TurnBasedGameResponse, new()
        {
            return new TResponse
            {
                m_command = request.m_command,
                m_player1 = data.m_player1 ?? "",
                m_player2 = data.m_player2 ?? "",
                m_userID = request.m_userID,
                m_success = true
            };
        }
        
        private async ValueTask JoinGame(User user, TurnBasedGameRequest request)
        {
            using var dataToken = await GetData();
            var data = dataToken.m_value;
            
            // require the data lock to ensure nobody joins during recycle
            await user.MoveTo(m_room);
            
            if (data.m_player1 == user.m_name || data.m_player2 == user.m_name)
            {
                throw new InvalidDataException("umm... you are already playing");
            }
            
            if (data.m_player1 == null)
            {
                data.m_player1 = user.m_name;
            } else if (data.m_player2 == null)
            {
                data.m_player2 = user.m_name;
            }
            
            var joinedAsPlayer = 
                data.m_player1 == user.m_name ||
                data.m_player2 == user.m_name;
            
            if (!data.m_gameStarted && joinedAsPlayer && data.m_player1 != null && data.m_player2 != null)
            {
                // starting
                data.m_currentPlayer = data.m_player1;
            }
            
            var resp = MakeResponse<TurnBasedGameJoinResponse>(request, data);
            resp.m_success = joinedAsPlayer;
            resp.m_gameStart = data.m_gameStarted;
            resp.m_joinData = "blehh";
            
            if (joinedAsPlayer)
            {
                await m_room.BroadcastXtRes(resp);
            } else
            {
                resp.m_gameData = data.Serialize();
                await user.BroadcastXtRes(resp);
            }
        }
        
        protected abstract TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, T data);
        protected virtual ValueTask HandlePlayerWinsRequest(TurnBasedGameRequest baseRequest)
        {
            throw new NotImplementedException("this game is server-authoritative");
        }
        
        private async ValueTask HandleTurnRequest(TurnBasedGameRequest request)
        {
            using var dataToken = await GetData();
            var data = dataToken.m_value;
            
            if (data.m_currentPlayer != request.m_userID)
            {
                throw new InvalidDataException("user taking turn that isn't theirs");
            }
            
            var response = TakeTurn(request, data);
            
            // can't be a stalemate if someone won
            response.m_staleMate &= !response.m_winnerFound;
            if (response.m_success)
            {
                if (response.m_nextPlayer == null) throw new NullReferenceException("m_nextPlayer not assigned");
                data.m_currentPlayer = response.m_nextPlayer;
            }
            
            await m_room.BroadcastXtRes(response);
            
            if (response.m_staleMate || response.m_winnerFound)
            {
                await Recycle(data);
            }
        }
        
        protected async ValueTask Recycle(T data)
        {
            data.Reset();
            
            m_recycling = true;
            await m_room.RemoveAllUsers();
            m_recycling = false;
        }

        public async ValueTask UserLeftRoom(Room room, User user)
        {
            if (m_recycling)
            {
                // if we are recycling the lock is already taken
                return;
            }
            
            using var dataToken = await GetData();
            var data = dataToken.m_value;
            
            if (data.m_player1 != user.m_name && data.m_player2 != user.m_name)
            {
                // doesn't matter, not a player
                return;
            }

            if (!data.m_gameStarted)
            {
                // no need to notify, game isnt running
                
                if (data.m_player1 == user.m_name) data.m_player1 = null;
                if (data.m_player2 == user.m_name) data.m_player2 = null;
                return;
            }

            await m_room.BroadcastXtRes(new TurnBasedGameNotification
            {
                m_command = Modules.TURN_BASED_USER_QUIT,
                m_userID = user.m_name
            });
            await Recycle(data);
        }
    }
}