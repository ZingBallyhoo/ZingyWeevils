using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct KartJoinGameRequest
    {
        [StrField] public string m_trackName;
        [StrField] public byte m_numPlayers;
        [StrField] public byte m_kartID;
        [StrField] public string m_kartColor;
    }
}