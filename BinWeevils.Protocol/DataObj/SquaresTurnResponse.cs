using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record SquaresTurnResponse : TurnBasedGameTurnResponse
    {
        [PropertyShape(Name = "row1")] public int m_row1;
        [PropertyShape(Name = "col1")] public int m_col1;
        [PropertyShape(Name = "row2")] public int m_row2;
        [PropertyShape(Name = "col2")] public int m_col2;
        [PropertyShape(Name = "keepingPlay")] public bool m_keepingPlay;
        [PropertyShape(Name = "winner")] public string? m_winner;
    }
}