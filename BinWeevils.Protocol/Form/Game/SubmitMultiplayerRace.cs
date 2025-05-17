using PolyType;

namespace BinWeevils.Protocol.Form.Game
{
    [GenerateShape]
    public partial class SubmitMultiplayerRaceRequest
    {
        [PropertyShape(Name = "key")] public string m_authKey;
        [PropertyShape(Name = "numBeaten")] public byte m_numBeaten;
    }
}