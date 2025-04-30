using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetSpecialMovesRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
}