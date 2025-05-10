using BinWeevils.Protocol.Enums;
using PolyType;

namespace BinWeevils.Protocol.Form.Garden
{
    [GenerateShape]
    public partial class UpgradeGardenRequest
    {
        [PropertyShape(Name = "size")] public EGardenSize m_size;
    }
}