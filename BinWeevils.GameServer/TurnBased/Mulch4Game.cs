using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;
using CommunityToolkit.HighPerformance;

namespace BinWeevils.GameServer.TurnBased
{
    public class Mulch4Game : TurnBasedGame<Mulch4GameData>
    {
        public Mulch4Game(Room room) : base(room)
        {
        }

        protected override TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, Mulch4GameData data)
        {
            var request = (Mulch4TakeTurnRequest)baseRequest;
            
            var columnData = data.m_columns[request.m_column];
            // todo: why is cast required :(
            
            var turnResponse = MakeResponse<Mulch4TurnResponse>(request, data);
            
            var firstEmptyRow = columnData.AsSpan().Cast<Mulch4GameData.TileState, byte>().IndexOf((byte)Mulch4GameData.TileState.Empty);
            if (firstEmptyRow == -1)
            {
                // can't place there :(
                turnResponse.m_success = false;
                return turnResponse;
            }
            
            var isPlayer1 = request.m_userID == data.m_player1;
            
            columnData[firstEmptyRow] = isPlayer1 ? Mulch4GameData.TileState.Player1 : Mulch4GameData.TileState.Player2;
            turnResponse.m_nextPlayer = isPlayer1 ? data.m_player2! : data.m_player1!;
            turnResponse.m_col = request.m_column;
            turnResponse.m_row = firstEmptyRow;
            
            var winningSlots = data.FindWinningSlots(request.m_column, firstEmptyRow);
            turnResponse.m_winnerFound = winningSlots != null;
            turnResponse.m_winningSlots = winningSlots == null ? null : string.Join(',', winningSlots);
            turnResponse.m_staleMate = data.m_columns.All(x => x.All(y => y != Mulch4GameData.TileState.Empty));
            
            return turnResponse;
        }
    }
}