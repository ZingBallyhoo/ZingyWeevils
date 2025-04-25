using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class ChangeWeevilDefResponse
    {
        [FormUrlEncodedPropertyName("err")] public int m_error { get; set; }
        
        public const int ERR_OK = 1;
        public const int ERR_DEF_IN_USE = 4;
        public const int ERR_NOT_ENOUGH_MONEY = 13;
        public const int ERR_MUST_BE_TYCOON = 15;
        public const int ERR_MUST_ACTIVATE = 72;
    }
}