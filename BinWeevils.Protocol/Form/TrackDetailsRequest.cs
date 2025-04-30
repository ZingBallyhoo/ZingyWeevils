using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class TrackDetailsRequest
    {
        [PropertyShape(Name = "trackID")] public int m_trackID { get; set; }
    }
}