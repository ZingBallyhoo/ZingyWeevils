using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.XmlMessages;
using Proto;

namespace BinWeevils.GameServer
{
    public class WeevilData : VarBag
    {
        public readonly User m_user;
        private readonly string m_contextAddress;
        
        public TypedVar<uint> m_idx;
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
        
        public string? m_buddyLocName = null;
        
        // todo: king, but unused...
        
        // todo: petDef
        // todo: petState

        public WeevilData(User user, IRootContext context)
        {
            m_user = user;
            m_contextAddress = context.System.Address;
            
            m_idx = new TypedVar<uint>(this, "idx", Var.TYPE_STRING); // ! intentionally string
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
            
            m_weevilDef.SetValue(WeevilDef.DEFAULT);
        }
        
        public int CalculateNewDirection(double xStart, double zStart, double x, double z)
        {
            var dx = x - xStart;
            var dz = z - xStart;
            if(dx == 0 && dz == 0)
            {
                return m_r.GetValue(); // unchanged
            }
            
            return (int)double.RadiansToDegrees(Math.Atan2(-dx, -dz));
        }
        
        public int CalculateNewDirection(double x, double z)
        {
            return CalculateNewDirection(m_x.GetValue(), m_z.GetValue(), x, z);
        }
        
        public PID GetUserAddress()
        {
            return new PID(m_contextAddress, m_user.m_name);
        }
        
        public PID GetNestAddress()
        {
            return new PID(m_contextAddress, $"{m_user.m_name}/nest");
        }
    }
}