using PolyType;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class UpdateJugglingTrickRequest
    {
        [PropertyShape(Name = "petID")] public uint m_petID;
        [PropertyShape(Name = "jugglingSkill")] public double m_jugglingSkill;
        
        [PropertyShape(Name = "trickID")] public uint m_trickID;
        [PropertyShape(Name = "aptitude")] public byte m_aptitude;
    }
}