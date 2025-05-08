using PolyType;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class UpdatePetSkillRequest
    {
        [PropertyShape(Name = "petID")] public uint m_petID;
        [PropertyShape(Name = "skillID")] public EPetSkill m_skillID;
        [PropertyShape(Name = "obedience")] public byte m_obedience;
        [PropertyShape(Name = "skillLevel")] public double m_skillLevel;
    }
}