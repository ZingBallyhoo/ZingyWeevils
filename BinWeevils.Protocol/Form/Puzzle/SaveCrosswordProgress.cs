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
        [PropertyShape(Name = "res")] public int m_result { get; set; }
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set; }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set; }
                
        public const int RESULT_COMPLETED = 2;
    }
}