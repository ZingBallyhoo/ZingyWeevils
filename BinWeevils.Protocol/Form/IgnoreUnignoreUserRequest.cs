using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class IgnoreUnignoreUserRequest
    {
        [PropertyShape(Name = "username")] public string m_userName;
    }
}