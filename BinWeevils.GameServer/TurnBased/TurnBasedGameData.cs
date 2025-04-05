namespace BinWeevils.GameServer.TurnBased
{
    public class TurnBasedGameData
    {
        public string? m_player1;
        public string? m_player2;
        public string? m_currentPlayer;
        
        public bool m_gameStarted => m_currentPlayer != null;
        
        public virtual void Reset()
        {
            m_player1 = null;
            m_player2 = null;
            m_currentPlayer = null;
        }
    }
}