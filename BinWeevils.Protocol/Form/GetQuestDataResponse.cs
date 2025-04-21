using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class GetQuestDataResponse
    {
        [FormUrlEncodedPropertyName("tasks")] public List<int> m_tasks { get; set; }
        [FormUrlEncodedPropertyName("itemList")] public string m_itemList { get; set; } = "";
        // "b=r"... why?
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
    }
}