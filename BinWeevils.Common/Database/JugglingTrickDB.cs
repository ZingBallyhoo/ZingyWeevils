using System.ComponentModel.DataAnnotations;

namespace BinWeevils.Common.Database
{
    public class JugglingTrickDB
    {
        [Key] public uint m_id { get; set; }
        
        public byte m_numBalls { get; set; }
        public string m_name { get; set; }
        
        public uint m_difficulty { get; set; }
        public string m_pattern { get; set; }
    }
}