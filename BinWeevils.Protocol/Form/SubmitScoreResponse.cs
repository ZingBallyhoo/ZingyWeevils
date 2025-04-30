using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class SubmitScoreResponse
    {
        [PropertyShape(Name = "result")] public int m_result { get; set; }
        [PropertyShape(Name = "mulchEarned")] public uint m_mulchEarned { get; set; }
        [PropertyShape(Name = "xpEarned")] public uint m_xpEarned { get; set; }
        
        public const int ERR_OK = 1;
        public const int ERR_INVALID_USER = 2;
        public const int ERR_PLAYED_ALREADY = 3;
        public const int ERR_WRONG_GAME = 4;
        public const int ERR_HASH = 5;
        public const int ERR_DB = 6;
        public const int ERR_PARAM = 7;
    }
}