namespace BinWeevils.Protocol.Json
{
    public class ScrapedManifest
    {
        public required Dictionary<int, ScrapedTaskData> m_tasks { get; set; }
        public required Dictionary<int, ScrapedQuest> m_quests { get; set; }
        public required HashSet<int> m_unavailableTasks { get; set; }
    }
    
    public class ScrapedQuest
    {
        public required List<int> m_tasks { get; set; }
    }
    
    public class ScrapedTaskData
    {
        public required int m_id { get; set; }
        public required int? m_questID { get; set; }

        public required int m_rewardMulch { get; set; }
        public required int m_rewardDosh { get; set; }
        public required int m_rewardXp { get; set; }

        public required string? m_rewardItemBundle { get; set; }
        public required List<ScrapedItemGain>? m_rewardBundleItems { get; set; }
        public required List<ScrapedItemGain>? m_rewardItems { get; set; }
        public required List<ScrapedItemGain>? m_rewardGardenItems { get; set; }
        public required Dictionary<string, int>? m_rewardGardenSeeds { get; set; }

        public required List<int>? m_rewardAchievements { get; set; }
        public required List<int>? m_rewardMoves { get; set; }

        public required List<int> m_deletedTasks { get; set; }

        public required bool m_nonRestartable { get; set; }
    }
    
    public class ScrapedItemGain : ScrapedItemKey
    {
        public required int m_count { get; set; }
    }

    public class ScrapedItemKey
    {
        public required string m_configName { get; set; }
        public required string m_color { get; set; }
    }
}