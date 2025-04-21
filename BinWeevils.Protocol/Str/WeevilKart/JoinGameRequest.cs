using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public ref partial struct JoinGameRequest
    {
        [StrField] public ReadOnlySpan<char> m_trackName;
        [StrField] public int m_numPlayers;
        [StrField] public int m_kartID;
        [StrField] public ReadOnlySpan<char> m_kartColor;
    }
}