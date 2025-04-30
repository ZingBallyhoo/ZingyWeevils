using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class PurchaseNestRoomRequest
    {
        [PropertyShape(Name = "roomID")] public ENestRoom m_roomType { get; set; }
    }
}