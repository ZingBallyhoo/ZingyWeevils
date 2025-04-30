using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class TaskCompletedResponse
    {
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
        [PropertyShape(Name = "res")] public int m_result { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
        // dosh...
        [PropertyShape(Name = "itemName")] public List<string> m_itemName { get; set; } = [];
        [PropertyShape(Name = "gardenItemName")] public List<string> m_gardenItemName { get; set; } = [];
        [PropertyShape(Name = "move")] public List<int> m_move { get; set; } = [];
        [PropertyShape(Name = "deleted")] public List<int> m_deletedTasks { get; set; } = [];
        [PropertyShape(Name = "completedAchievements")] public List<int> m_completedAchievements { get; set; } = [];
        [PropertyShape(Name = "bundleName")] public string m_bundleName { get; set; } = "";
        
        // who cooked
        
        public const int RESPONSE_NOT_FOUND = 0;
        public const int RESPONSE_OK = 1;
        public const int RESPONSE_MISSION_ALREADY_ACTIVE = 999;
        
        public const int RES_TASK_COMPLETE = 1;
        public const int RES_QUEST_COMPLETE = 2;
    }
}