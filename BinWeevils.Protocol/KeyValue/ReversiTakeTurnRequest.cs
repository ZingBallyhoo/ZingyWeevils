using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record ReversiTakeTurnRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "row")] public int m_row;
        [PropertyShape(Name = "col")] public int m_col;
    }
}