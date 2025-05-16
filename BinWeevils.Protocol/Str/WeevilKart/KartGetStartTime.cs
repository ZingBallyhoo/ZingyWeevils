using StackXML.Str;

namespace BinWeevils.Protocol.Str.WeevilKart
{
    public partial record struct  KartGetStartTimeRequest : IStrClass
    {
        public void Serialize(ref StrWriter writer)
        {
        }

        public void Deserialize(ref StrReader reader)
        {
        }
    }
    
    public partial record struct KartGetStartTimeResponse
    {
        [StrField] public uint m_time;
    }
}