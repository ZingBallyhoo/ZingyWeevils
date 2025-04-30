using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetQuestDataResponse
    {
        [PropertyShape(Name = "tasks")] public List<int> m_tasks { get; set; }
        [PropertyShape(Name = "itemList")] public string m_itemList { get; set; } = "";
        // "b=r"... why?
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
    }
}