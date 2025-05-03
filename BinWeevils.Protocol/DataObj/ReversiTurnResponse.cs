using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record ReversiTurnResponse : TurnBasedGameTurnResponse
    {
        [PropertyShape(Name = "row")] public int m_row;
        [PropertyShape(Name = "col")] public int m_col;
        [PropertyShape(Name = "keepingPlay")] public bool m_keepingPlay;
        [PropertyShape(Name = "winner")] public string? m_winner;
    }
}