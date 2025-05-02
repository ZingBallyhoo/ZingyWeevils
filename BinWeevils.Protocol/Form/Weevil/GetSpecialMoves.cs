using PolyType;

namespace BinWeevils.Protocol.Form.Weevil
{
    [GenerateShape]
    public partial class GetSpecialMovesRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
    
    [GenerateShape]
    public partial class GetSpecialMovesResponse
    {
        [PropertyShape(Name = "responseCode")] public int m_responseCode { get; set; }
        [PropertyShape(Name = "result")] public string m_result { get; set; } // delimited by ";"
    }
}