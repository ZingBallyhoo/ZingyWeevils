using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct ClientPetAction
    {
        [StrField] public uint m_petID;
        [StrField] public int m_actionID;
        [StrField] public string m_stateVars;
        [StrField] public string m_extraParams;
    }
}