namespace BinWeevils.GameServer.TurnBased
{
    public abstract class TurnBasedGameData
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
        
        public abstract string Serialize();
        
        public string GetOtherPlayer(string player)
        {
            if (m_player1 == player) return m_player2!;
            if (m_player2 == player) return m_player1!;
            throw new InvalidDataException("game doesn't have 2 players");
        }
    }
}