using PolyType;

namespace BinWeevils.Protocol.Form.Puzzle
{
    [GenerateShape]
    public partial class SaveCrosswordProgressRequest
    {
        [PropertyShape(Name = "gridID")] public byte m_gridID { get; set; }
        [PropertyShape(Name = "completed")] public bool m_completed { get; set; }
        [PropertyShape(Name = "progress")] public string m_progress { get; set; }
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
}