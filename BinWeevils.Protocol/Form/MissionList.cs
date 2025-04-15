using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public record MissionList
    {
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
        [FormUrlEncodedPropertyName("idList")] public string m_ids { get; set; }
        [FormUrlEncodedPropertyName("nameList")] public string m_names { get; set; }
        [FormUrlEncodedPropertyName("pathList")] public string m_paths { get; set; }
        [FormUrlEncodedPropertyName("levelList")] public string levelList { get; set; }
        [FormUrlEncodedPropertyName("completedList")] public string completedList { get; set; }
        [FormUrlEncodedPropertyName("tycoonList")] public string tycoonList { get; set; }
        [FormUrlEncodedPropertyName("roomList")] public string roomList { get; set; }
        [FormUrlEncodedPropertyName("priceList")] public string priceList { get; set; }
        [FormUrlEncodedPropertyName("scoreBronze")] public string scoreBronze { get; set; }
        [FormUrlEncodedPropertyName("scoreSilver")] public string scoreSilver { get; set; }
        [FormUrlEncodedPropertyName("scoreGold")] public string scoreGold { get; set; }
        [FormUrlEncodedPropertyName("scorePlatinum")] public string scorePlatinum { get; set; }
        [FormUrlEncodedPropertyName("highScore")] public string highScore { get; set; }
    }
}