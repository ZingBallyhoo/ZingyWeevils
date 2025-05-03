using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public class ReversiGameData : TileBasedGameData
    {
        public ReversiGameData() : base(8, 8)
        {
        }
        
        public bool TakeTurn(int row, int col, TileState ourState)
        {
            var currentState = m_columns[col][row];
            if (currentState != TileState.Empty)
            {
                return false;
            }
            
            var tilesToFlip = new HashSet<(int, int)>();
            CollectTilesToFlip(tilesToFlip, row, col, ourState);
            if (tilesToFlip.Count == 0)
            {
                return false;
            }
            
            m_columns[col][row] = ourState;
            foreach (var tuple in tilesToFlip)
            {
                m_columns[tuple.Item2][tuple.Item1] = ourState;
            }
            return true;
        }
        
        private void CollectTilesToFlip(HashSet<(int, int)> set, int row, int col, TileState ourState)
        {
            CollectTilesToFlip(set, row, col, 1, 0, ourState);
            CollectTilesToFlip(set, row, col, -1, 0, ourState);
            CollectTilesToFlip(set, row, col, 0, 1, ourState);
            CollectTilesToFlip(set, row, col, 0, -1, ourState);
            
            CollectTilesToFlip(set, row, col, 1, 1, ourState);
            CollectTilesToFlip(set, row, col, -1, 1, ourState);
            CollectTilesToFlip(set, row, col, 1, -1, ourState);
            CollectTilesToFlip(set, row, col, -1, -1, ourState);
        }
        
        private void CollectTilesToFlip(HashSet<(int, int)> set, int startRow, int startCol, int rowStep, int colStep, TileState ourState)
        {
            startRow += rowStep;
            startCol += colStep;
            var row = startRow;
            var col = startCol;
            while (true)
            {
                if (row < 0 || row >= m_numRows) return; // can't continue
                if (col < 0 || col >= m_numColumns) return; // can't continue
                
                var foundState = m_columns[col][row];
                if (foundState == TileState.Empty) return; // can't continue
                
                if (foundState == ourState)
                {
                    // go ahead and commit anything we found
                    break;
                }
                
                // otherwise, it's their state :)
                // continue the search
                row += rowStep;
                col += colStep;
            }

            // if we reached here, we matched something
            while (startRow != row || startCol != col)
            {
                set.Add((startRow, startCol));
                startRow += rowStep;
                startCol += colStep;
            }
        }
        
        public bool ShouldKeepPlay(TileState ourState) 
        {
            var theirState = ourState == TileState.Player1 ? 
                TileState.Player2 : 
                TileState.Player1;
            
            var tilesToFlip = new HashSet<(int, int)>();
            for (var col = 0; col < m_numColumns; col++)
            {
                for (var row = 0; row < m_numRows; row++)
                {
                    if (m_columns[col][row] != TileState.Empty) continue;
                    
                    tilesToFlip.Clear();
                    CollectTilesToFlip(tilesToFlip, row, col, theirState);
                    if (tilesToFlip.Count > 0)
                    {
                        // they can play
                        return false;
                    }
                }
            }
            
            // other player has no valid plays :((
            return true;
        }
        
        public void SetWinState(ReversiTurnResponse response) 
        {
            var count1 = 0;
            var count2 = 0;
            var countEmpty = 0;

            foreach (var row in m_columns)
            {
                foreach (var state in row)
                {
                    if (state == TileState.Player1) count1++;
                    if (state == TileState.Player2) count2++;
                    if (state == TileState.Empty) countEmpty++;
                }
            }
            
            if (count1 == 0)
            {
                response.m_winner = m_player2!;
                return;
            }
            if (count2 == 0)
            {
                response.m_winner = m_player1!;
                return;
            }
            
            if (countEmpty != 0) return;
            if (count1 == count2)
            {
                response.m_staleMate = true;
                return;
            }
            if (count1 > count2)
            {
                response.m_winner = m_player1!;
            } else
            {
                response.m_winner = m_player2!;
            }
        }

        public override void Reset()
        {
            base.Reset();
            
            m_columns[3][3] = TileState.Player1;
            m_columns[3][4] = TileState.Player2;
            m_columns[4][4] = TileState.Player1;
            m_columns[4][3] = TileState.Player2;
        }
    }
    
    public class ReversiGame : TurnBasedGame<ReversiGameData>
    {
        public ReversiGame(Room room) : base(room)
        {
        }

        protected override TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, ReversiGameData data)
        {
            var request = (ReversiTakeTurnRequest)baseRequest;
            
            var isPlayer1 = request.m_userID == data.m_player1;
            var ourState = isPlayer1 ? TileBasedGameData.TileState.Player1 : TileBasedGameData.TileState.Player2;
            if (!data.TakeTurn(request.m_row, request.m_col, ourState)) 
            {
                var failedResp = MakeResponse<ReversiTurnResponse>(request, data);
                failedResp.m_success = false;
                return failedResp;
            }
            
            var resp = MakeResponse<ReversiTurnResponse>(request, data);
            resp.m_row = request.m_row;
            resp.m_col = request.m_col;
            resp.m_keepingPlay = data.ShouldKeepPlay(ourState);
            resp.m_nextPlayer = resp.m_keepingPlay ? request.m_userID : data.GetOtherPlayer(request.m_userID);
            data.SetWinState(resp);
            resp.m_winnerFound = resp.m_winner != null;
            resp.m_success = true;
            return resp;
        }
    }
}