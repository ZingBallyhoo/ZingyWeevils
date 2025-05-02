using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class TrackDetailsRequest
    {
        [PropertyShape(Name = "trackID")] public int m_trackID { get; set; }
    }
    
    [GenerateShape]
    public partial class TrackDetailsResponse
    {
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
        [PropertyShape(Name = "file")] public string m_file { get; set; }
        [PropertyShape(Name = "title")] public string m_title { get; set; }
        [PropertyShape(Name = "artist")] public string m_artist { get; set; }
        [PropertyShape(Name = "trackID")] public int m_trackID { get; set; }
    }
}