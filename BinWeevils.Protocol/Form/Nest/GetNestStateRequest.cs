using PolyType;

namespace BinWeevils.Protocol.Form.Nest
{
    [GenerateShape]
    public partial class GetNestStateRequest
    {
        [PropertyShape(Name = "id")] public string m_userName { get; set; }
    }
}