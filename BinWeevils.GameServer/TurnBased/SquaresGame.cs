using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public class SquaresGameData : TurnBasedGameData
    {
        public int m_player1SquareCount;
        public int m_player2SquareCount;
        
        public override string Serialize()
        {
            // todo:
            return "";
        }

        public override void Reset()
        {
            m_player1SquareCount = 0;
            m_player2SquareCount = 0;
            base.Reset();
        }
    }
    
    public class SquaresGame : TurnBasedGame<SquaresGameData>
    {
        public SquaresGame(Room room) : base(room)
        {
        }

        protected override TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, SquaresGameData data)
        {
            var request = (SquaresTakeTurnRequest)baseRequest;
            
            if (request.m_player1SquareCount < data.m_player1SquareCount ||
                request.m_player2SquareCount < data.m_player2SquareCount)
            {
                throw new InvalidDataException("number of squares decreasing");
            }
            
            if (request.m_player1SquareCount - data.m_player1SquareCount > 2 || 
                request.m_player2SquareCount - data.m_player2SquareCount > 2)
            {
                throw new InvalidDataException("trying to add >2 squares at once");
            }
            
            var isPlayer1 = request.m_userID == data.m_player1;
            if (isPlayer1)
            {
                if (request.m_player2SquareCount != data.m_player2SquareCount)
                {
                    throw new InvalidDataException("player 1 changing squares for player 2");
                }
            } else
            {
                if (request.m_player1SquareCount != data.m_player1SquareCount)
                {
                    throw new InvalidDataException("player 2 changing squares for player 1");
                }
            }
            
            var nextPlayerIsUs = request.m_nextPlayer == request.m_userID;
            if (nextPlayerIsUs != request.m_keepingPlay)
            {
                throw new InvalidDataException("lying about keeping play");
            }
            if (request.m_keepingPlay && request.m_player1SquareCount == data.m_player1SquareCount && request.m_player2SquareCount == data.m_player2SquareCount)
            {
                throw new InvalidDataException("cant keep play if you didnt capture any squares");
            }
            
            // todo: actually validate the board state...
            
            data.m_player1SquareCount = request.m_player1SquareCount;
            data.m_player2SquareCount = request.m_player2SquareCount;
            
            var resp = MakeResponse<SquaresTurnResponse>(baseRequest, data);
            resp.m_row1 = request.m_row1;
            resp.m_col1 = request.m_col1;
            resp.m_row2 = request.m_row2;
            resp.m_col2 = request.m_col2;
            resp.m_keepingPlay = request.m_keepingPlay;
            resp.m_nextPlayer = request.m_nextPlayer;
            resp.m_winnerFound = request.m_player1SquareCount + request.m_player2SquareCount == 5*5;
            if (resp.m_winnerFound)
            {
                resp.m_winner = data.m_player1SquareCount > data.m_player2SquareCount ?
                    data.m_player1 : data.m_player2;
            }
            resp.m_success = true;
            return resp;
        }
    }
}