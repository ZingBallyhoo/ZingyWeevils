using BinWeevils.Protocol.Str;
using PolyType;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleTurnBasedCommand(in XtClientMessage message, ref StrReader reader)
        {
            var turnedBasedDataStr = reader.GetString();
            var turnBasedData = new Dictionary<string, string>();
            
            // todo: how can this be parsed properly?
            // the client is sending whatever it wants depending on game+command...
            // lookahead?
            
            ReadOnlySpan<char> gameTypeID = default;
            ReadOnlySpan<char> command = default;

            foreach (var varRange in turnedBasedDataStr.Split(','))
            {
                var varSpan = turnedBasedDataStr[varRange];
                if (varSpan.Length == 0) continue; // trailing comma
                
                var indexOfColon = varSpan.IndexOf(',');
                
                var nameSpan = varSpan.Slice(0, indexOfColon);
                var valueSpan = varSpan.Slice(indexOfColon+1);
                
                if (nameSpan is "gameTypeID")
                {
                    gameTypeID = valueSpan;
                } else if (nameSpan is "command")
                {
                    command = valueSpan;
                }
                
                turnBasedData.Add(nameSpan.ToString(), valueSpan.ToString());
            }
        }
    }
    
    [GenerateShape]
    public partial record TurnBasedGameRequest
    {
        [PropertyShape(Name = "uniqueGameSessionID")] public string m_uniqueGameSessionID;
        [PropertyShape(Name = "gameTypeID")] public string m_gameTypeID;
        [PropertyShape(Name = "command")] public string m_command;
        [PropertyShape(Name = "gameSessionID")] public string m_gameSessionID;
        [PropertyShape(Name = "userID")] public string m_userID;
        [PropertyShape(Name = "slot")] public int m_slot;
    }
    
    [GenerateShape]
    public partial record Mulch4TakeTurnRequest : TurnBasedGameRequest
    {
        [PropertyShape(Name = "col")] public int m_column;
    }
}