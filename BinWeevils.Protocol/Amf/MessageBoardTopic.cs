using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class MessageBoardTopic
    {
        // todo: is column order correct?
        public uint m_topicID;
        public string m_weevilID;
        public uint m_boardID;
        public string m_title;
        public string m_message;
        public string m_dateStarted;
        public uint m_replies;
        public uint m_views;
        public uint m_active;
        public bool m_sticky;
        public string m_lastReply;
        public bool m_closed;
    }
}