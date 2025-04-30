using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class ChangeWeevilDefRequest
    {
        [PropertyShape(Name = "weevilDef")] public ulong m_weevilDef { get; set; }
    }
}