using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class GetStoredItemsRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
        [PropertyShape(Name = "mine")] public bool m_mine { get; set; }
    }
}