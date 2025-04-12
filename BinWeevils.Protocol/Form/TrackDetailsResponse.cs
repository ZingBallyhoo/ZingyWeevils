using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class TrackDetailsResponse
    {
        [FormUrlEncodedPropertyName("responseCode")] public int m_responseCode { get; set; }
        [FormUrlEncodedPropertyName("file")] public string m_file { get; set; }
        [FormUrlEncodedPropertyName("title")] public string m_title { get; set; }
        [FormUrlEncodedPropertyName("artist")] public string m_artist { get; set; }
        [FormUrlEncodedPropertyName("trackID")] public int m_trackID { get; set; }
    }
}