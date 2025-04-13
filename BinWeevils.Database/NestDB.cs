using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Sql;

namespace BinWeevils.Database
{
    [Table("NestDB")]
    public class NestDB
    {
        [Key] public int m_id { get; set; }
        [ConcurrencyCheck] public DateTime m_lastUpdated { get; set; }
        
        public virtual ICollection<NestRoomDB> m_rooms { get; set; }
        
        public virtual ICollection<NestItemDB> m_items { get; set; }
        
        public static NestDB Empty()
        {
            return new NestDB
            {
                m_lastUpdated = DateTime.Now,
                m_rooms = [
                    new NestRoomDB
                    {
                        m_type = ENestRoom.Room4
                    },
                    new NestRoomDB
                    {
                        m_type = ENestRoom.Garden
                    },
                    new NestRoomDB
                    {
                        m_type = ENestRoom.Hall
                    },
                    new NestRoomDB
                    {
                        m_type = ENestRoom.VODRoom
                    },
                    new NestRoomDB
                    {
                        m_type = ENestRoom.Plaza
                    }
                ],
                m_items = []
            };
        }
    }
    
    public class NestRoomDB
    {
        [Key] public int m_id { get; set; }
        [Required] public ENestRoom m_type { get; set; } // maps to id in xml...
        [Required] public string m_color { get; set; } = "0|0|0";
        
        public int m_nestID;
        [Required] public virtual NestDB m_nest { get; set; }
    }
    
    public class NestItemDB
    {
        [Key] public int m_id { get; set; }
        // todo: color...
        
        [Required] public virtual ItemType m_itemType { get; set; }
        [Required] public virtual NestDB m_nest { get; set; }
        
        public virtual NestPlacedItemDB? m_placedItem { get; set; }

    }
    
    public class NestPlacedItemDB
    {
        // todo: could share key? but its not required
        [Key] public int m_id { get; set; }
        
        public int m_roomID { get; set; }
        public int m_currentPos { get; set; }

        public int m_placedOnFurnitureID { get; set; }
        public int m_spotOnFurniture { get; set; }
        
        [Required] public virtual NestRoomDB m_room { get; set; }
        public virtual NestPlacedItemDB? m_placedOnFurniture { get; set; } // ornaments can get placed on furniture
    }
}