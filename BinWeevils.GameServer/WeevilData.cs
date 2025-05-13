using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.KeyValue;
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
        public TypedVar<byte> m_doorID;
        public TypedVar<int> m_poseID;
        public TypedVar<byte> m_expressionID;
        public TypedVar<int> m_victor;
        public TypedVar<double> m_x;
        public TypedVar<double> m_y;
        public TypedVar<double> m_z;
        public TypedVar<int> m_r;
        // todo: king, but unused...
        public TypedVar<string> m_petDef;
        public TypedVar<string> m_petState;
        
        public string? m_buddyLocName = null;
        
        public HashSet<uint> m_myPetIDs = [];
        public HashSet<string> m_myPetNames = [];

        public WeevilData(User user, IRootContext context)
        {
            m_user = user;
            m_contextAddress = context.System.Address;
            
            m_idx = new TypedVar<uint>(this, "idx", Var.TYPE_STRING); // ! intentionally string
            m_weevilDef = new TypedVar<ulong>(this, "weevilDef", Var.TYPE_STRING);
            m_apparel = new TypedVar<string>(this, "apparel", Var.TYPE_STRING);
            m_locID = new TypedVar<int>(this, "locID", Var.TYPE_NUMBER);
            m_doorID = new TypedVar<byte>(this, "doorID", Var.TYPE_NUMBER);
            m_poseID = new TypedVar<int>(this, "ps", Var.TYPE_NUMBER);
            m_expressionID = new TypedVar<byte>(this, "ex", Var.TYPE_NUMBER);
            m_victor = new TypedVar<int>(this, "vict", Var.TYPE_STRING); // ! intentionally string
            m_x = new TypedVar<double>(this, "x", Var.TYPE_NUMBER);
            m_y = new TypedVar<double>(this, "y", Var.TYPE_NUMBER);
            m_z = new TypedVar<double>(this, "z", Var.TYPE_NUMBER);
            m_r = new TypedVar<int>(this, "r", Var.TYPE_NUMBER);
            m_petDef = new TypedVar<string>(this, "petDef", Var.TYPE_STRING);
            m_petState = new TypedVar<string>(this, "petState", Var.TYPE_STRING);
            
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
        
        public void AddPet(PetDefVar var)
        {
            m_petState.SetValue(new PetStateVar
            {
                m_pose = EPetAction.JUMP_ON
            }.ToString());
            m_petDef.SetValue(var.ToString());
        }
        
        public void RemovePet()
        {
            m_petDef.SetValue("");
        }
        
        public void SetPetState(PetStateVar state)
        {
            m_petState.SetValue(state.ToString());
        }
        
        public PID GetUserAddress()
        {
            return new PID(m_contextAddress, m_user.m_name);
        }
        
        public PID GetNestAddress()
        {
            return new PID(m_contextAddress, $"{m_user.m_name}/nest");
        }
        public PID GetPetManagerAddress()
        {
            return new PID(m_contextAddress, $"{m_user.m_name}/petManager");
        }
    }
}