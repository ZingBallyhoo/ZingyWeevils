using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Common.Database
{
    public class JugglingTrickDB
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public uint m_id { get; set; }
        
        public byte m_numBalls { get; set; }
        public string m_name { get; set; }
        
        public uint m_difficulty { get; set; }
        public string m_pattern { get; set; }
    }
}