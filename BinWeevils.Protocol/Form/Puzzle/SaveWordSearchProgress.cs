using BinWeevils.Protocol.Str;
using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SaveWordSearchProgressRequest
    {
        [PropertyShape(Name = "gridID")] public byte m_gridID { get; set; }
        [PropertyShape(Name = "completed")] public bool m_completed { get; set; }
        [PropertyShape(Name = "progress")] public WordSearchProgress m_progress { get; set; }
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
}