using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class GetStoredGardenItemsRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
    }
}