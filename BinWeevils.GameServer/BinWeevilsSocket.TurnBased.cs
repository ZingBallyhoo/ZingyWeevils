using BinWeevils.GameServer.PolyType;
using BinWeevils.GameServer.TurnBased;
using BinWeevils.Protocol;
using BinWeevils.Protocol.KeyValue;
using BinWeevils.Protocol.Str;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleTurnBasedCommand(in XtClientMessage message, ref StrReader reader)
        {
            var turnedBasedDataStr = reader.GetString();
            Console.Out.WriteLine($"turn based: {turnedBasedDataStr}");
            
            var request = ParseTurnBasedRequest(turnedBasedDataStr);
            
            m_taskQueue.Enqueue(async () =>
            {
                var user = GetUser();
                var mainRoom = await user.GetRoom();
                var gameRoom = await user.m_zone.GetRoom($"TurnBased_{mainRoom.m_name}_{request.m_slot}");
                var turnBasedGame = gameRoom!.GetData<TurnBasedGame>();
                await turnBasedGame.IncomingRequest(user, request);
            });
        }
        
        private static bool TryParseGameAndCommand(ReadOnlySpan<char> request, out ReadOnlySpan<char> gameTypeID, out ReadOnlySpan<char> command)
        {
            gameTypeID = default;
            command = default;
            
            // todo: just search..?

            foreach (var varRange in request.Split(','))
            {
                var varSpan = request[varRange];
                if (varSpan.Length == 0) continue; // trailing comma
                
                var indexOfColon = varSpan.IndexOf(':');
                
                var nameSpan = varSpan.Slice(0, indexOfColon);
                var valueSpan = varSpan.Slice(indexOfColon+1);
                
                if (nameSpan is "gameTypeID")
                {
                    gameTypeID = valueSpan;
                } else if (nameSpan is "command")
                {
                    command = valueSpan;
                }
            }
            
            return gameTypeID.Length != 0 && command.Length != 0;
        }
        
        private static TurnBasedGameRequest ParseTurnBasedRequest(ReadOnlySpan<char> request)
        {
            if (!TryParseGameAndCommand(request, out var gameTypeID, out var command))
            {
                throw new Exception("cant find game & command id");
            }
                
            switch (gameTypeID)
            {
                case "mulch4":
                {
                    break;
                }
            }

            switch (command)
            {
                case Modules.TURN_BASED_JOIN:
                case Modules.TURN_BASED_REMOVE_PLAYER:
                {
                    return KeyValueDeserializer.Deserialize<TurnBasedGameRequest>(request);
                }
                default:
                {
                    throw new NotImplementedException($"unknown base command {command} - {request}");
                }
            }
        }
    }
}