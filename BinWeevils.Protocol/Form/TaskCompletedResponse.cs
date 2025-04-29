using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class TaskCompletedResponse
    {
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
        [FormUrlEncodedPropertyName("res")] public int m_result { get; set; }
        [FormUrlEncodedPropertyName("mulch")] public int m_mulch { get; set; }
        [FormUrlEncodedPropertyName("xp")] public uint m_xp { get; set; }
        // dosh...
        [FormUrlEncodedPropertyName("itemName")] public List<string> m_itemName { get; set; } = [];
        [FormUrlEncodedPropertyName("gardenItemName")] public List<string> m_gardenItemName { get; set; } = [];
        [FormUrlEncodedPropertyName("move")] public List<int> m_move { get; set; } = [];
        [FormUrlEncodedPropertyName("deleted")] public List<int> m_deletedTasks { get; set; } = [];
        [FormUrlEncodedPropertyName("completedAchievements")] public List<int> m_completedAchievements { get; set; } = [];
        [FormUrlEncodedPropertyName("bundleName")] public string m_bundleName { get; set; } = "";
        
        // who cooked
        
        public const int RESPONSE_NOT_FOUND = 0;
        public const int RESPONSE_OK = 1;
        public const int RESPONSE_MISSION_ALREADY_ACTIVE = 999;
        
        public const int RES_TASK_COMPLETE = 1;
        public const int RES_QUEST_COMPLETE = 2;
    }
}