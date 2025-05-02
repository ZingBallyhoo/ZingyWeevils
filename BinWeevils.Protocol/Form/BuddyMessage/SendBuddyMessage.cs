using PolyType;

namespace BinWeevils.Protocol.Form.BuddyMessage
{
    [GenerateShape]
    public partial class SendBuddyMessageRequest
    {
        [PropertyShape(Name = "msg")] public string m_message { get; set; }
        [PropertyShape(Name = "recipIDX")] public uint m_recipientIdx { get; set; }
        [PropertyShape(Name = "hash")] public string m_hash { get; set; }
    }
}