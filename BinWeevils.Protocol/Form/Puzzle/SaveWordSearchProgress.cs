using BinWeevils.Protocol.Str;
using PolyType;

namespace BinWeevils.Protocol.Form.Puzzle
{
    [GenerateShape]
    public partial class SaveWordSearchProgressRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
        [PropertyShape(Name = "gridID")] public byte m_gridID { get; set; }
        [PropertyShape(Name = "progress")] public WordSearchProgress m_progress { get; set; }
        [PropertyShape(Name = "completed")] public bool m_completed { get; set; }
    }

    [GenerateShape]
    public partial class SaveWordSearchProgressResponse
    {
        [PropertyShape(Name = "mulch")] public int m_mulch { get; set;  }
        [PropertyShape(Name = "xp")] public uint m_xp { get; set;  }
    }
}