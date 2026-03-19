using PolyType;

namespace BinWeevils.Protocol.Form.Puzzle
{
    [GenerateShape]
    public partial class SaveCrosswordProgressRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
        [PropertyShape(Name = "gridID")] public byte m_gridID { get; set; }
        [PropertyShape(Name = "progress")] public string m_progress { get; set; }
        [PropertyShape(Name = "completed")] public bool m_completed { get; set; }
    }

    [GenerateShape]
    public partial class SaveCrosswordProgressResponse
    {
        // todo
    }
}