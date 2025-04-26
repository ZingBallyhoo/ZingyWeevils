using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class SubmitScoreRequest
    {
        [FormUrlEncodedPropertyName("gameID")] public uint m_gameID { get; set; }
        [FormUrlEncodedPropertyName("score")] public uint m_score { get; set; }
        
        public const int GAME_SPOTDIFFERENCE = 2;
    }
}