using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record BallTakeTurnRequest: TurnBasedGameRequest
    {
        [PropertyShape(Name = "nextPlayer")] public string m_nextPlayer;

        [PropertyShape(Name = "id")] public int m_ballID;
        [PropertyShape(Name = "dirx")] public double m_dirX;
        [PropertyShape(Name = "diry")] public double m_dirY;
        [PropertyShape(Name = "x")] public double m_x;
        [PropertyShape(Name = "y")] public double m_y;
        
        [PropertyShape(Name = "ids")] public string m_what1;
        [PropertyShape(Name = "poss")] public string m_what2;
    }
}