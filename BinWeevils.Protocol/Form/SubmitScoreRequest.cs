using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitScoreRequest
    {
        [PropertyShape(Name = "gameID")] public uint m_gameID { get; set; }
        [PropertyShape(Name = "score")] public uint m_score { get; set; }
        
        public const int GAME_SPOTDIFFERENCE = 2;
    }
}