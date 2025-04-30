using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class WeevilDataRequest
    {
        [PropertyShape(Name = "id")] public string m_name { get; set; }
    }
}