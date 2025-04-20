using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class TaskCompletedRequest
    {
        [FormUrlEncodedPropertyName("userID")] public string m_userName { get; set; }
        [FormUrlEncodedPropertyName("questID")] public int m_questID { get; set; }
        [FormUrlEncodedPropertyName("taskID")] public int m_taskID { get; set; }
    }
}