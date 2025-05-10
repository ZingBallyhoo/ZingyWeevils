using PolyType;
using StackXML.Str;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class GetPetJugglingTricksRequest
    {
        [PropertyShape(Name = "petID")] public uint m_petID;
    }
    
    [GenerateShape]
    public partial class GetPetJugglingTricksResponse 
    {
        [PropertyShape(Name = "result")] public string m_resultTricks;
    }
    
    [GenerateShape]
    public partial record struct DelimitedJugglingTrick
    {
        [StrField] public uint m_trickID;
        [StrField] public byte m_numBalls;
        [StrField] public string m_pattern;
        [StrField] public uint m_difficulty;
        [StrField] public double m_aptitude;
    }
}