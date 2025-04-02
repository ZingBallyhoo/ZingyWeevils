using BinWeevils.Protocol.XmlMessages;

namespace BinWeevils.GameServer.Sfs
{
    public class VarBag : Dictionary<string, Var>
    {
        public void UpdateVar(Var var)
        {
            this[var.m_name] = var;
        }
        
        public List<Var> GetVars()
        {
            return Values.ToList();
        }
    }
}