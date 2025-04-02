using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial struct ClientAction
    {
        [StrField] public int m_actionID;
        [StrField] public int m_endPoseID;
        [StrField] public string m_extraParams;
    }
    
    public partial struct ServerAction
    {
        [StrField] public int m_userID;
        [StrField] public int m_actionID;
        [StrField] public string m_extraParams;
    }
}