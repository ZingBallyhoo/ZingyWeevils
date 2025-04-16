using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct NestJoinedLocNotification
    {
        [StrField] public int m_userID;
        [StrField] public NestJoinLocRequest m_body;
    }
}