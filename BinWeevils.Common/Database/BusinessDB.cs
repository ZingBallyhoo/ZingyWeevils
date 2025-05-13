using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Xml;

namespace BinWeevils.Common.Database
{
    public class BusinessDB
    {
        [Key] public uint m_id { get; set; }
        
        public string m_name { get; set; } = "";
        public bool m_open { get; set; }
        public EBusinessType m_type { get; set; }
        public int m_signColor { get; set; } = 0xFFFFFF;
        public int m_signTextColor { get; set; } = 0;
        public PlayListIDs m_playList { get; set; } = new PlayListIDs();
        
        [ForeignKey(nameof(m_id))] public virtual NestRoomDB m_room { get; set; }
    }
}