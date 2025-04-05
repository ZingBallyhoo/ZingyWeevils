using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record TurnBasedGameResponse : XtResponse
    {
        [PropertyShape(Name = "command")] public string m_command;
        [PropertyShape(Name = "player1")] public string m_player1;
        [PropertyShape(Name = "player2")] public string m_player2;
        [PropertyShape(Name = "userID")] public string m_userID;
        
        public TurnBasedGameResponse()
        {
            m_commandType = Modules.TURN_BASED;
        }
    }
}