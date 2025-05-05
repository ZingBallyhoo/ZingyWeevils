using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetCommand
    {
        [StrField] public string m_petName;
        [StrField] public string m_petNameHash;
        [StrField] public byte m_commandID;
    }
    
    public partial record struct ServerPetCommand
    {
        [StrField] public ulong m_userID;
        [StrField] public string m_petName;
        [StrField] public byte m_commandID;
    }
}