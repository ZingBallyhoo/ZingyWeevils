using StackXML;

namespace BinWeevils.Protocol.XmlMessages
{
    [XmlCls("body")]
    public partial class SetUserVarsRequest : MsgBody
    {
        [XmlBody] public VarList m_vars = new VarList();
    }
}