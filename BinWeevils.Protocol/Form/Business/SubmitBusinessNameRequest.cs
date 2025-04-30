using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class SubmitBusinessNameRequest
    {
        [PropertyShape(Name = "locID")] public uint m_locID { get; set; }
        [PropertyShape(Name = "busName")] public string m_name { get; set ;}
    }
}