using PolyType;
using StackXML.Str;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class GetPetSkillsRequest
    {
        [PropertyShape(Name = "petID")] public uint m_petID;
    }
    
    [GenerateShape]
    public partial class GetPetSkillsResponse 
    {
        [PropertyShape(Name = "result")] public string m_resultSkills;
    }
    
    public partial record struct DelimitedPetSkill
    {
        [StrField] public uint m_skillID;
        [StrField] public byte m_obedience;
        [StrField] public double m_skillLevel;
    }
}