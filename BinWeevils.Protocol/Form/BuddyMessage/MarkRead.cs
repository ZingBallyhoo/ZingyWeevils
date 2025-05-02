using PolyType;

namespace BinWeevils.Protocol.Form.BuddyMessage
{
    [GenerateShape]
    public partial class MarkReadRequest
    {
        [PropertyShape(Name = "id")] public uint m_id { get; set; }
    }
}