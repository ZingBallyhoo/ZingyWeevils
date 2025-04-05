using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record BallJoinRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "count")] public int m_ballCount;
    }
}