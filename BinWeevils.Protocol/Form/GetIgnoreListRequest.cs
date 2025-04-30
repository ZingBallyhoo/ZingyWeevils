using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetIgnoreListRequest
    {
        [PropertyShape(Name = "userID")] public string m_userName { get; set; }
    }
}