using System.Text;

namespace BinWeevils.GameServer.TurnBased
{
    public abstract class TileBasedGameData : TurnBasedGameData
    {
        protected readonly int m_numRows;
        protected readonly int m_numColumns;
        public readonly TileState[][] m_columns;
        
        public enum TileState : byte
        {
            Empty,
            Player1,
            Player2,
        }

        protected TileBasedGameData(int rows, int columns) 
        {
            m_numRows = rows;
            m_numColumns = columns;
            
            m_columns = new TileState[m_numColumns][];
            for (var i = 0; i < m_columns.Length; i++)
            {
                m_columns[i] = new TileState[m_numRows];
            }
        }
        
        public override string Serialize()
        {
            var sb = new StringBuilder();
            for (var col = 0; col < m_numColumns; col++)
            {
                var colData = m_columns[col];
                
                for (var row = 0; row < m_numRows; row++)
                {
                    var tileOwner = colData[row] switch
                    {
                        TileState.Empty => "-1",
                        TileState.Player1 => m_player1,
                        TileState.Player2 => m_player2,
                        _ => throw new InvalidDataException()
                    };
                    
                    sb.Append($"{row}_{col}:{tileOwner},");
                }
            }
            
            return sb.ToString();
        }
        
        public override void Reset()
        {
            base.Reset();
            foreach (var column in m_columns)
            {
                column.AsSpan().Fill(TileState.Empty);
            }
        }
    }
}