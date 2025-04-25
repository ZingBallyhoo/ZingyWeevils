using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class ChangeWeevilDefRequest
    {
        [FormUrlEncodedPropertyName("weevilDef")] public ulong m_weevilDef { get; set; }
    }
}