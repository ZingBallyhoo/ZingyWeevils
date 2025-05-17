using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class SubmitTimeTrialRequest
    {
        [PropertyShape(Name = "trackID")] public byte m_trackID;
        [PropertyShape(Name = "time")] public uint m_time;
    }
}