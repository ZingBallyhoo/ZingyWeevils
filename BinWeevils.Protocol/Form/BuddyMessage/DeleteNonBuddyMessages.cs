using PolyType;

namespace BinWeevils.Protocol.Form.BuddyMessage
{
    [GenerateShape]
    public partial class DeleteNonBuddyMessagesRequest
    {
        [PropertyShape(Name = "ids")] public List<uint> m_ids;
    }
}