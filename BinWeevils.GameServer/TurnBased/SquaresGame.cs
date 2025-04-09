using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.KeyValue;

namespace BinWeevils.GameServer.TurnBased
{
    public class SquaresGameData : TurnBasedGameData
    {
        public override string Serialize()
        {
            // todo:
            return "";
        }
    }
    
    public class SquaresGame : TurnBasedGame<SquaresGameData>
    {
        public SquaresGame(Room room) : base(room)
        {
        }

        protected override TurnBasedGameTurnResponse TakeTurn(TurnBasedGameRequest baseRequest, SquaresGameData data)
        {
            var resp = MakeResponse<TurnBasedGameTurnResponse>(baseRequest, data);
            resp.m_success = false;
            return resp;
        }
    }
}