using PolyType;

namespace BinWeevils.Protocol.Form.Puzzle
{
    [GenerateShape]
    public partial class GetPuzzleProgressRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
        [PropertyShape(Name = "gridID")] public byte m_gridID { get; set; }
    }
    
    [GenerateShape]
    public partial class GetCrosswordProgressResponse
    {
        [PropertyShape(Name = "prog")] public string m_progress { get; set; }
        [PropertyShape(Name = "completed")] public bool m_completed { get; set; }
    }
}