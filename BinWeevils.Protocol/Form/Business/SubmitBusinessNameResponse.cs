using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Business
{
    public class SubmitBusinessNameResponse
    {
        [FormUrlEncodedPropertyName("res")] public string m_result { get; set; }
        
        public const string RESULT_TAKEN = "1";
        public const string RESULT_SUCCESS = "2";
    }
}