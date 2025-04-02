using BinWeevils.Protocol.XmlMessages;

namespace BinWeevils.GameServer.Sfs
{
    public struct TypedVar<T> where T : IEquatable<T>
    {
        private readonly VarBag m_bag;
        private Var m_var;
        private T m_cachedValue;
        
        public TypedVar(VarBag bag, string name, string type)
        {
            m_bag = bag;
            
            m_var = new Var(name, type, null!);
            if (typeof(T) == typeof(string))
            {
                ForceSetValue((T)(object)"");
            } else
            {
                ForceSetValue(default!);
            }
        }
        
        public void SetValue(T value)
        {
            if (m_cachedValue.Equals(value)) return;
            ForceSetValue(value);
        }
        
        private void ForceSetValue(T value)
        {
            m_cachedValue = value;
            m_var.m_value = $"{value}";
            
            m_bag.UpdateVar(m_var);
        }
        
        public T GetValue()
        {
            return m_cachedValue;
        }
        
        public static implicit operator T(TypedVar<T> h)
        {
            return h.GetValue();
        }
    }
}