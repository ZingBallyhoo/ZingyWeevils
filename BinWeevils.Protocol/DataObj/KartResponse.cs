using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record KartNotification : XtNotification
    {
        [PropertyShape(Name = "command")] public string m_command;
    }
}