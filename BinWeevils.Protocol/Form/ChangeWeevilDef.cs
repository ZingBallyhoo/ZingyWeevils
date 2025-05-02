using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class ChangeWeevilDefRequest
    {
        [PropertyShape(Name = "weevilDef")] public ulong m_weevilDef { get; set; }
    }
    
    [GenerateShape]
    public partial class ChangeWeevilDefResponse
    {
        [PropertyShape(Name = "err")] public int m_error { get; set; }
        
        public const int ERR_OK = 1;
        public const int ERR_DEF_IN_USE = 4;
        public const int ERR_NOT_ENOUGH_MONEY = 13;
        public const int ERR_MUST_BE_TYCOON = 15;
        public const int ERR_MUST_ACTIVATE = 72;
    }
}