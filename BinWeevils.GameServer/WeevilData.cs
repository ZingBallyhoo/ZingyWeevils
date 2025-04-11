using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.XmlMessages;

namespace BinWeevils.GameServer
{
    public class WeevilData : VarBag
    {
        public readonly User m_user;
        
        public TypedVar<int> m_idx;
        public TypedVar<ulong> m_weevilDef;
        public TypedVar<string> m_apparel;
        public TypedVar<int> m_locID;
        public TypedVar<int> m_doorID;
        public TypedVar<int> m_poseID;
        public TypedVar<int> m_expressionID;
        public TypedVar<int> m_victor;
        public TypedVar<double> m_x;
        public TypedVar<double> m_y;
        public TypedVar<double> m_z;
        public TypedVar<int> m_r;
        
        // todo: king, but unused...
        
        // todo: petDef
        // todo: petState

        public WeevilData(User user)
        {
            m_user = user;
            
            m_idx = new TypedVar<int>(this, "idx", Var.TYPE_STRING); // ! intentionally string
            m_weevilDef = new TypedVar<ulong>(this, "weevilDef", Var.TYPE_STRING);
            m_apparel = new TypedVar<string>(this, "apparel", Var.TYPE_STRING);
            m_locID = new TypedVar<int>(this, "locID", Var.TYPE_NUMBER);
            m_doorID = new TypedVar<int>(this, "doorID", Var.TYPE_NUMBER);
            m_poseID = new TypedVar<int>(this, "ps", Var.TYPE_NUMBER);
            m_expressionID = new TypedVar<int>(this, "ex", Var.TYPE_NUMBER);
            m_victor = new TypedVar<int>(this, "vict", Var.TYPE_STRING); // ! intentionally string
            m_x = new TypedVar<double>(this, "x", Var.TYPE_NUMBER);
            m_y = new TypedVar<double>(this, "y", Var.TYPE_NUMBER);
            m_z = new TypedVar<double>(this, "z", Var.TYPE_NUMBER);
            m_r = new TypedVar<int>(this, "r", Var.TYPE_NUMBER);
            
            m_weevilDef.SetValue(ulong.Parse(WeevilDef.DEFAULT));
        }
    }
}