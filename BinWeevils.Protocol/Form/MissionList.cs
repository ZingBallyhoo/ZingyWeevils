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
        [PropertyShape(Name = "levelList")] public string levelList { get; set; }
        [PropertyShape(Name = "completedList")] public string completedList { get; set; }
        [PropertyShape(Name = "tycoonList")] public string tycoonList { get; set; }
        [PropertyShape(Name = "roomList")] public string roomList { get; set; }
        [PropertyShape(Name = "priceList")] public string priceList { get; set; }
        [PropertyShape(Name = "scoreBronze")] public string scoreBronze { get; set; }
        [PropertyShape(Name = "scoreSilver")] public string scoreSilver { get; set; }
        [PropertyShape(Name = "scoreGold")] public string scoreGold { get; set; }
        [PropertyShape(Name = "scorePlatinum")] public string scorePlatinum { get; set; }
        [PropertyShape(Name = "highScore")] public string highScore { get; set; }
    }
}