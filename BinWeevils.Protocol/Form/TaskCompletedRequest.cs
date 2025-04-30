using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class TaskCompletedRequest
    {
        [PropertyShape(Name = "userID")] public string m_userName { get; set; }
        [PropertyShape(Name = "questID")] public int m_questID { get; set; }
        [PropertyShape(Name = "taskID")] public int m_taskID { get; set; }
    }
}