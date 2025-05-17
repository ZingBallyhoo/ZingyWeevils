using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class StartMultiplayerRaceRequest
    {
        [PropertyShape(Name = "trackId")] public byte m_trackID;
    }
    
    [GenerateShape]
    public partial class StartMultiplayerRaceResponse
    {
        [PropertyShape(Name = "key")] public string m_key;
    }
}