using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record Mulch4TakeTurnRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "col")] public int m_column;
    }
}