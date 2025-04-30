using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GeoResponse
    {
        [PropertyShape(Name = "l")] public string m_l { get; set; }
    }
}