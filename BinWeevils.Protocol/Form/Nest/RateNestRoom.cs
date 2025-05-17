using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class RateNestRoomRequest
    {
        [PropertyShape(Name = "locID")] public uint m_locID;
        [PropertyShape(Name = "rating")] public byte m_rating;
    }
}