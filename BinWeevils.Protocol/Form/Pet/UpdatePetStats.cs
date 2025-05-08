using PolyType;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class UpdatePetStatsRequest
    {
        [PropertyShape(Name = "petID")] public uint m_petID;
        [PropertyShape(Name = "mentalEnergy")] public byte m_mentalEnergy;
        [PropertyShape(Name = "fuel")] public byte m_fuel;
        [PropertyShape(Name = "health")] public byte m_health;
        [PropertyShape(Name = "fitness")] public byte m_fitness;
        [PropertyShape(Name = "experience")] public uint m_experience;
    }
}