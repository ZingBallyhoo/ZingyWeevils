using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record LoginResponse : XtResponse
    {
        [PropertyShape(Name = "user")] public LoginUser m_user;
        // todo: "fr"?
        // todo: "kd"?
    }
    
    [GenerateShape]
    public partial record LoginUser
    {
        // (yes, they are all strings)
        [PropertyShape(Name = "weevilDef")] public string m_weevilDef;
        [PropertyShape(Name = "ip")] public string m_ip;
        [PropertyShape(Name = "apparel")] public string m_apparel;
        [PropertyShape(Name = "idx")] public string m_idx;
        [PropertyShape(Name = "locale")] public string m_locale;
        [PropertyShape(Name = "userID")] public string m_userID;
    }
}