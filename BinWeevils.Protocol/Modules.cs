using System.Diagnostics.CodeAnalysis;

namespace BinWeevils.Protocol
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Modules
    {
        public const string CHAT = "1";
        public const string CHAT_YOURSELF = $"{CHAT}#1";
        public const string CHAT_CHANGE_STATE = $"{CHAT}#2";
        
        public const string INGAME = "2";
        public const string INGAME_MOVE = $"{INGAME}#1";
        public const string INGAME_EXPRESSION = $"{INGAME}#2";
        public const string INGAME_ACTION = $"{INGAME}#3";
        public const string INGAME_JOIN_ROOM = $"{INGAME}#4";
        public const string INGAME_ROOM_EVENT = $"{INGAME}#5";
        public const string INGAME_GET_ZONE_TIME = $"{INGAME}#6";
        public const string INGAME_ADD_APPAREL = $"{INGAME}#7";
        public const string INGAME_REMOVE_APPAREL = $"{INGAME}#8";
        
        // todo: not to be confused with the actual turn based module? (4)
        // although it seems unused
        public const string TURN_BASED = "tbmt";
        public const string TURN_BASED_JOIN = "j";
        public const string TURN_BASED_REMOVE_PLAYER = "rp";
        public const string TURN_BASED_TAKE_TURN = "tt";
        public const string TURN_BASED_USER_QUIT = "go"; // (game over)
        
        public const string DINER = "9";
        public const string DINER_GRAB_TRAY = $"{DINER}#1";
        public const string DINER_DROP_TRAY = $"{DINER}#2";
        public const string DINER_CHEF_START = $"{DINER}#3";
        public const string DINER_CHEF_QUIT = $"{DINER}#4";
    }
}