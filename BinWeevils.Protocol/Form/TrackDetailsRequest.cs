using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class TrackDetailsRequest
    {
        [FormUrlEncodedPropertyName("trackID")] public int m_trackID { get; set; }
    }
}