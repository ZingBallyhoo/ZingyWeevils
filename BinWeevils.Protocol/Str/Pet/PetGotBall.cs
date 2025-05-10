using StackXML.Str;

namespace BinWeevils.Protocol.Str.Pet
{
    public partial record struct PetGotBall
    {
        [StrField] public uint m_petID;
    }
}