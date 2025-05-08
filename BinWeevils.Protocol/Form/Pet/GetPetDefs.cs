using PolyType;
using StackXML.Str;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class GetPetDefsRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID;
    }
    
    [GenerateShape]
    public partial class GetPetDefsResponse
    {
        [PropertyShape(Name = "result")] public string m_resultDefs;
    }
    
    public partial record struct DelimitedPetDef
    {
        [StrField] public uint m_id;
        [StrField] public string m_name;
        [StrField] public uint m_bedID;
        [StrField] public uint m_bowlID;
        [StrField] public uint m_bodyColor;
        [StrField] public uint m_antenna1Color;
        [StrField] public uint m_antenna2Color;
        [StrField] public uint m_eye1Color;
        [StrField] public uint m_eye2Color;
        [StrField] public byte m_fuel;
        [StrField] public byte m_mentalEnergy;
        [StrField] public byte m_health;
        [StrField] public byte m_fitness;
        [StrField] public uint m_experience;
        [StrField] public string m_nameHash;
    }
}