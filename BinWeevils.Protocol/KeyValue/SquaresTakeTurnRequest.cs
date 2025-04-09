using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record SquaresTakeTurnRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "nextPlayer")] public string m_nextPlayer;

        [PropertyShape(Name = "row1")] public int m_row1;
        [PropertyShape(Name = "col1")] public int m_col1;
        [PropertyShape(Name = "row2")] public int m_row2;
        [PropertyShape(Name = "col2")] public int m_col2;
        [PropertyShape(Name = "p1sc")] public int m_player1SquareCount;
        [PropertyShape(Name = "p2sc")] public int m_player2SquareCount;
        
        [PropertyShape(Name = "keepingPlay")] public bool m_keepingPlay;
    }
}