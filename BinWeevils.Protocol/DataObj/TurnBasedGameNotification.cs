using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record TurnBasedGameNotification : XtNotification
    {
        [PropertyShape(Name = "command")] public string m_command;
        [PropertyShape(Name = "userID")] public string m_userID;
    }
}