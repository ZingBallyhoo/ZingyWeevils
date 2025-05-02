using PolyType;

namespace BinWeevils.Protocol.Form.BuddyMessage
{
    [GenerateShape]
    public partial class HasUnreadBuddyMessageResponse
    {
        [PropertyShape(Name = "res")] public int m_result;
    }
}