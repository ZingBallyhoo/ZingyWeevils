using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BinWeevils.GameServer
{
    public static class GameServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.GameServer");
        public static readonly Meter s_meter = new Meter("BinWeevils.GameServer");
        
        public static readonly Counter<int> s_packetsReceived = s_meter.CreateCounter<int>("bw_packets_received");
        public static readonly Counter<int> s_packetsSent = s_meter.CreateCounter<int>("bw_packets_sent");
        public static readonly Counter<int> s_loginAttempts = s_meter.CreateCounter<int>("bw_login_attempts");
        public static readonly Counter<int> s_usersCreated = s_meter.CreateCounter<int>("bw_users_created");
        
        public static readonly Counter<int> s_roomsJoined = s_meter.CreateCounter<int>("bw_rooms_joined");
        public static readonly Counter<int> s_chatMessagesSent = s_meter.CreateCounter<int>("bw_chat_messages_sent");
        
        public static readonly Counter<int> s_movesSent = s_meter.CreateCounter<int>("bw_socket_moves_sent");
        public static readonly Counter<int> s_roomEventsSent = s_meter.CreateCounter<int>("bw_socket_room_events_sent");
        public static readonly Counter<int> s_actionsSent = s_meter.CreateCounter<int>("bw_socket_actions_sent");
        public static readonly Counter<int> s_petCommandsSent = s_meter.CreateCounter<int>("bw_socket_pet_commands_sent");
        public static readonly Counter<int> s_petActionsSent = s_meter.CreateCounter<int>("bw_socket_pet_actions_sent");
        public static readonly Counter<int> s_petExpressionsSent = s_meter.CreateCounter<int>("bw_socket_pet_expressions_sent");
        
        public static readonly Counter<int> s_turnBasedGamesStarted = s_meter.CreateCounter<int>("bw_turn_based_games_started");
        public static readonly Counter<int> s_turnBasedGamesFinished = s_meter.CreateCounter<int>("bw_turn_based_games_finished");
        public static readonly Counter<int> s_turnBasedGamesAbandoned = s_meter.CreateCounter<int>("bw_turn_based_games_abandoned");
        
        public static readonly Counter<int> s_kartGamesStarted = s_meter.CreateCounter<int>("bw_kart_games_started");
        public static readonly Counter<int> s_kartPlayersAbandoned = s_meter.CreateCounter<int>("bw_kart_players_abandoned");
        public static readonly Counter<int> s_kartPlayersCompleted = s_meter.CreateCounter<int>("bw_kart_players_completed");
        
        public static readonly Counter<int> s_dinerTraysGrabbed = s_meter.CreateCounter<int>("bw_diner_trays_grabbed");
        public static readonly Counter<int> s_dinerChefsStarted = s_meter.CreateCounter<int>("bw_diner_chefs_started");
        public static readonly Counter<int> s_dinerFoodSet = s_meter.CreateCounter<int>("bw_diner_food_set");
        public static readonly Counter<int> s_dinerFoodEaten = s_meter.CreateCounter<int>("bw_diner_food_eaten");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
    }
}