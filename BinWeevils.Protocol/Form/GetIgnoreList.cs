using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetIgnoreListRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID;
    }
    
    [GenerateShape]
    public partial class GetIgnoreListResponse
    {
        [PropertyShape(Name = "result")] public List<string> m_resultNames;
    }
}