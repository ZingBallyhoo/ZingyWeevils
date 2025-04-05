using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record TurnBasedGameRequest
    {
        [PropertyShape(Name = "command")] public string m_command;
        [PropertyShape(Name = "gameTypeID")] public string m_gameTypeID;
        [PropertyShape(Name = "slot")] public int m_slot;
        
        [PropertyShape(Name = "uniqueGameSessionID")] public string m_uniqueGameSessionID;
        [PropertyShape(Name = "gameSessionID")] public string? m_gameSessionID;

        [PropertyShape(Name = "userID")] public string m_userID;
    }
}