using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public class BallGameData : TurnBasedGameData
    {
        public override string Serialize()
        {
            // not supported, spectators wait for the next turn
            return "";
        }
        
        // todo: join data - random ball offset?
    }
    
    public class BallGame : TurnBasedGame<BallGameData>
    {
        public BallGame(Room room) : base(room)
        {
        }

        protected override TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, BallGameData data)
        {
            var request = (BallTakeTurnRequest)baseRequest;
            
            if (request.m_nextPlayer == data.m_currentPlayer)
            {
                throw new InvalidDataException("turns should be alternating");
            }
            
            var response = MakeResponse<BallTurnResponse>(baseRequest, data);
            response.m_nextPlayer = request.m_nextPlayer;
            response.m_ballID = request.m_ballID;
            response.m_dirX = request.m_dirX;
            response.m_dirY = request.m_dirY;
            response.m_x = request.m_x;
            response.m_y = request.m_y;
            response.m_what1 = request.m_what1;
            response.m_what2 = request.m_what2;
            return response;
        }

        protected override async ValueTask HandlePlayerWinsRequest(TurnBasedGameRequest baseRequest)
        {
            var winRequest = (BallWinGameRequest)baseRequest;
            if (winRequest.m_userID != winRequest.m_userWinner)
            {
                throw new InvalidDataException("only the winner sends this"); // (i think)
            }
            if (winRequest.m_userLoser == winRequest.m_userWinner)
            {
                throw new InvalidDataException("winner and loser can't be the same user");
            }
            
            await m_room.BroadcastXtRes(new BallGameEndedNotification
            {
                m_command = baseRequest.m_command,
                m_loserMulch = 50,
                m_winnerMulch = 100,
                m_userLoser = winRequest.m_userLoser,
                m_userWinner = winRequest.m_userWinner
            });
            
            using var dataToken = await GetData();
            await Recycle(dataToken.m_value);
        }
    }
}