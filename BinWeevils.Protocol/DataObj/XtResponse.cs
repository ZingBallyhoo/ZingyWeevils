using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record XtResponse : XtNotification
    {
        [PropertyShape(Name = "success")] public bool m_success;
    }
}