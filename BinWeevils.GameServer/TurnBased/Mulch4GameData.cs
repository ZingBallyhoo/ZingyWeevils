namespace BinWeevils.GameServer.TurnBased
{
    public class Mulch4GameData : TurnBasedGameData
    {
        public readonly BoardState[][] m_columns;
        
        private const int COLUMNS = 7;
        private const int ROWS = 6;
        private const int SEQUENCE = 4;
        
        public enum BoardState : byte
        {
            Empty,
            Player1,
            Player2,
        }
        
        public Mulch4GameData()
        {
            m_columns = new BoardState[COLUMNS][];
            for (var i = 0; i < m_columns.Length; i++)
            {
                m_columns[i] = new BoardState[ROWS];
            }
        }
        
        public List<string>? FindWinningSlots(int column, int row)
        {
            return FindWinningSlots(column, row, 1, 0) ??
                   FindWinningSlots(column, row, 0, 1) ??
                   FindWinningSlots(column, row, 1, 1);
        }
        
        private List<string>? FindWinningSlots(int startColumn, int startRow, int colStep, int rowStep)
        {
            var numAhead = 0;
            var numBehind = 0;
            
            var desired = m_columns[startColumn][startRow];
            if (desired == BoardState.Empty) throw new InvalidDataException();
            
            var column = startColumn - colStep;
            var row = startRow - rowStep;
            while (column >= 0 && row >= 0)
            {
                var curr = m_columns[column][row];
                if (curr != desired) break;
                
                numBehind++;
                
                column -= colStep;
                row -= rowStep;
            }
            
            var sequenceStartCol = column+colStep;
            var sequenceStartRow = row+rowStep;

            column = startColumn;
            row = startRow;
            while (column < COLUMNS && row < ROWS)
            {
                var curr = m_columns[column][row];
                if (curr != desired) break;
                
                numAhead++;
                
                column += colStep;
                row += rowStep;
            }
            
            var sequenceCount = numAhead + numBehind;
            var isWin = sequenceCount >= SEQUENCE;
            if (!isWin)
            {
                return null;
            }
            
            var winningSlots = new List<string>();
            for (var i = 0; i < sequenceCount; i++)
            {
                winningSlots.Add($"{sequenceStartCol+i*colStep}:{sequenceStartRow+i*rowStep}");
            }
            return winningSlots;
        }
        
        public override void Reset()
        {
            base.Reset();
            foreach (var column in m_columns)
            {
                column.AsSpan().Fill(BoardState.Empty);
            }
        }
    }
}