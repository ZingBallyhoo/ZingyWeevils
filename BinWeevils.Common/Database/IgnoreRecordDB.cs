using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Common.Database
{
    public class IgnoreRecordDB
    {
        // two users can both block, the block records have no relation to each-other
        [Key] public int m_id { get; set; }
        
        public uint m_forWeevilIdx { get; set; } 
        public uint m_ignoredWeevilIdx { get; set; }

        [ForeignKey(nameof(m_forWeevilIdx))] public WeevilDB m_forWeevil { get; set; }
        [ForeignKey(nameof(m_ignoredWeevilIdx))] public WeevilDB m_ignoredWeevil { get; set; }
    }
}