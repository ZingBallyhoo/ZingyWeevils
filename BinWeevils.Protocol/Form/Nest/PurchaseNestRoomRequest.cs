using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form.Nest
{
    public class PurchaseNestRoomRequest
    {
        [FormUrlEncodedPropertyName("roomID")] public ENestRoom m_roomType { get; set; }
    }
}