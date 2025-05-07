using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class GetUserBuddyCountRequest
    {
        public string m_userID;
        public string m_zone;
    }
}