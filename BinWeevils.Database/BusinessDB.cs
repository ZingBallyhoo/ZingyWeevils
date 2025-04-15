using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol;

namespace BinWeevils.Database
{
    public class BusinessDB
    {
        [Key] public uint m_id { get; set; }
        public uint m_roomID { get; set; }
        
        public string m_name { get; set; } = "";
        public bool m_open { get; set; }
        public EBusinessType m_type { get; set; }
        public uint m_signColor { get; set; } = 0xFFFFFF;
        public uint m_signTextColor { get; set; } = 0;
        
        [Required, ForeignKey(nameof(m_roomID))] public virtual NestRoomDB m_room { get; set; }
    }
}