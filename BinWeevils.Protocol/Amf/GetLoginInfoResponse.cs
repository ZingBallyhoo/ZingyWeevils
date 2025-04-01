using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class GetLoginDetailsResponse
    {
        [PropertyShape(Name = "userName")] public string m_userName;
        [PropertyShape(Name = "userIDX")] public int m_userIdx;
        [PropertyShape(Name = "tycoon")] public int m_tycoon;
        [PropertyShape(Name = "loginKey")] public string m_loginKey;
    }
}