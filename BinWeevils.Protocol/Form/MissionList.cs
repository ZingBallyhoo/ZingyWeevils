using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial record MissionList
    {
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
        [PropertyShape(Name = "idList")] public string m_ids { get; set; }
        [PropertyShape(Name = "nameList")] public string m_names { get; set; }
        [PropertyShape(Name = "pathList")] public string m_paths { get; set; }
        [PropertyShape(Name = "levelList")] public string m_levels { get; set; }
        [PropertyShape(Name = "completedList")] public string m_completed { get; set; }
        [PropertyShape(Name = "tycoonList")] public string m_tycoon { get; set; }
        [PropertyShape(Name = "roomList")] public string m_room { get; set; }
        [PropertyShape(Name = "priceList")] public string m_prizeList { get; set; }
        [PropertyShape(Name = "scoreBronze")] public string m_scoreBronze { get; set; }
        [PropertyShape(Name = "scoreSilver")] public string m_scoreSilver { get; set; }
        [PropertyShape(Name = "scoreGold")] public string m_scoreGold { get; set; }
        [PropertyShape(Name = "scorePlatinum")] public string m_scorePlatinum { get; set; }
        [PropertyShape(Name = "highScore")] public string m_highScore { get; set; }
    }
}