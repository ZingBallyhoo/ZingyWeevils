using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class BuyPremisesRequest
    {
        [PropertyShape(Name = "locTypeID")] public uint m_locTypeID { get; set;}
    }
}