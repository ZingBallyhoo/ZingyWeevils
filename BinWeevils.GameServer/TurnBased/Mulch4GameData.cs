namespace BinWeevils.GameServer.TurnBased
{
    public class Mulch4GameData : TileBasedGameData
    {
        private const int SEQUENCE = 4;
        
        public Mulch4GameData() : base(6, 7)
        {
        }
        
        public List<string>? FindWinningSlots(int column, int row)
        {
            // todo: this algorithm isn't good enoug...
            // we should show multiple wins if there are any
            return FindWinningSlots(column, row, 1, 0) ??
                   FindWinningSlots(column, row, 0, 1) ??
                   FindWinningSlots(column, row, 1, 1) ??
                   FindWinningSlots(column, row, -1, 1);
        }
        
        private List<string>? FindWinningSlots(int startColumn, int startRow, int colStep, int rowStep)
        {
            var numAhead = 0;
            var numBehind = 0;
            
            var desired = m_columns[startColumn][startRow];
            if (desired == TileState.Empty) throw new InvalidDataException();
            
            var column = startColumn - colStep;
            var row = startRow - rowStep;
            while (column >= 0 && row >= 0 && column < m_numColumns && row < m_numRows)
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
            while (column >= 0 && row >= 0 && column < m_numColumns && row < m_numRows)
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
    }
}