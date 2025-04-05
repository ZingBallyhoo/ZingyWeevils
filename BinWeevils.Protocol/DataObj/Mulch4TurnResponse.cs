using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record Mulch4TurnResponse : TurnBasedGameResponse
    {
        [PropertyShape(Name = "col")] public int m_col;
        [PropertyShape(Name = "row")] public int m_row;
    }
}