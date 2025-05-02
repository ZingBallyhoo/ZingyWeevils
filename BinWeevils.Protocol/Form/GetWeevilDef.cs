using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetWeevilDefRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
    
    [GenerateShape]
    public partial class GetWeevilDefResponse
    {
        [PropertyShape(Name = "weevilDef")] public ulong m_weevilDef { get; set; }
    }
}