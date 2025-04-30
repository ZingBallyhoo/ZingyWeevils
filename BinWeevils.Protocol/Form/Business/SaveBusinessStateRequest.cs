using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class SaveBusinessStateRequest
    {
        [PropertyShape(Name = "locID")] public uint m_locID { get; set; }
        [PropertyShape(Name = "signClr")] public uint m_signColor { get; set; }
        [PropertyShape(Name = "signTxtClr")] public uint m_signTextColor { get; set; }
        [PropertyShape(Name = "busOpen")] public byte m_busOpen { get; set; }
    }
}