using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetGoHome
    {
        [StrField] public string m_petIDsVar;
        [StrField] public uint m_petID;
        [StrField] public string m_petState;
    }
    
    public partial record struct ServerPetGoHome
    {
        [StrField] public string m_petDef;
        [StrField] public string m_petState;
    }
}