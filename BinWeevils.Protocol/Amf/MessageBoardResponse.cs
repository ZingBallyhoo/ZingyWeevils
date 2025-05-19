using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class MessageBoardResponse
    {
        [PropertyShape(Name = "resObject")] public MessageBoardResultObject m_resultObject;
        [PropertyShape(Name = "numRows")] public int m_numRows;
    }
    
    [GenerateShape]
    public partial class MessageBoardResultObject
    {
        [PropertyShape(Name = "serverInfo")] public MessageBoardServerInfo m_serverInfo;
    }
    
    [GenerateShape]
    public partial class MessageBoardServerInfo
    {
        [PropertyShape(Name = "columnNames")] public string[] m_columnNames = [];
        [PropertyShape(Name = "initialData")] public object?[][] m_initialData = [];
    }
}