using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class StartMultiplayerRaceRequest
    {
        [PropertyShape(Name = "trackId")] public byte m_trackID;
    }
}