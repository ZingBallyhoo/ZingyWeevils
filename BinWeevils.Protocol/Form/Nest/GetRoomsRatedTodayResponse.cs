using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class GetRoomsRatedTodayResponse
    {
        [PropertyShape(Name = "ratedToday")] public List<uint> m_ratedToday;
    }
}