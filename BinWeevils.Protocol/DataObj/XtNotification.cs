using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record XtNotification
    {
        [PropertyShape(Name = "commandType")] public string m_commandType;
    }
}