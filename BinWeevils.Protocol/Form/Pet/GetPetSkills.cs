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
    
    [GenerateShape]
    public partial record struct DelimitedPetSkill
    {
        [PropertyShape(Name = "skillID")] [StrField] public uint m_skillID;
        [PropertyShape(Name = "obedience")] [StrField] public byte m_obedience;
        [PropertyShape(Name = "skillLevel")] [StrField] public double m_skillLevel;
    }
}