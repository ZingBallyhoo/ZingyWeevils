using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class SubmitBusinessNameResponse
    {
        [PropertyShape(Name = "res")] public string m_result { get; set; }
        
        public const string RESULT_TAKEN = "1";
        public const string RESULT_SUCCESS = "2";
    }
}