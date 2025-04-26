using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class LoginBody : MsgBody
    {
        [XmlCls("login")]
        public partial record struct Data
        {
            [XmlField("z")] public string m_zone;
            [XmlBody("nick")] public string m_nickname;
            [XmlBody("pword")] public string m_password;
        }

        [XmlBody] public Data m_data;
    }
}