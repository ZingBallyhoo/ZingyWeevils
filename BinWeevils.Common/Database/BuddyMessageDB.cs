using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Common.Database
{
    public class BuddyMessageDB
    {
        [Key] public uint m_id { get; set; }
        
        public uint m_to { get; set; }
        public uint m_from { get; set; }
        public string m_message { get; set; }
        public DateTime m_sentAt { get; set; }
        public bool m_read { get; set; }
        
        [Required, ForeignKey(nameof(m_to))] public WeevilDB m_toWeevil { get; set; }
        [Required, ForeignKey(nameof(m_from))] public WeevilDB m_fromWeevil { get; set; }
    }
}