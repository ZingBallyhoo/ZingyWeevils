using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Sql;
using BinWeevils.Protocol.Xml;

namespace BinWeevils.Common.Database
{
    [Table("NestDB")]
    public class NestDB
    {
        [Key] public uint m_id { get; set; }
        
        public EGardenSize m_gardenSize { get; set; } = EGardenSize.Regular;
        public uint m_fuel { get; set; } = 46807; // todo: pick a number..
        
        public DateTime m_lastUpdated { get; set; }
        
        private DateTime m_itemsLastUpdatedBacking;

        [ConcurrencyCheck]
        public DateTime m_itemsLastUpdated
        {
            get => m_itemsLastUpdatedBacking;
            set
            {
                m_lastUpdated = value;
                m_itemsLastUpdatedBacking = value;
            }
        }

        public virtual ICollection<NestRoomDB> m_rooms { get; set; } = [];
        public virtual ICollection<NestItemDB> m_items { get; set; } = [];
        public virtual ICollection<NestGardenItemDB> m_gardenItems { get; set; } = [];
        public virtual ICollection<NestSeedItemDB> m_gardenSeeds { get; set; } = [];
        
        public static NestDB Empty()
        {
            return new NestDB
            {
                m_itemsLastUpdated = DateTime.UtcNow,
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
        [Key] public uint m_id { get; set; }
        [Required] public ENestRoom m_type { get; set; } // maps to id in xml...
        [Required] public NestRoomColor m_color { get; set; }
        
        public uint m_nestID { get; set; }
        [Required, ForeignKey(nameof(m_nestID))] public virtual NestDB m_nest { get; set; }
        
        public virtual BusinessDB? m_business { get; set; }
    }
    
    public class NestItemDB
    {
        [Key] public uint m_id { get; set; }
        [Required] public uint m_nestID { get; set; }
        [Required] public uint m_itemTypeID { get; set; }
        [Required] public ItemColor m_color { get; set; } = new ItemColor();
        
        public virtual NestPlacedItemDB? m_placedItem { get; set; }
        
        [Required, ForeignKey(nameof(m_nestID))] public virtual NestDB m_nest { get; set; }
        [Required, ForeignKey(nameof(m_itemTypeID))] public virtual ItemType m_itemType { get; set; }

    }
    
    public class NestPlacedItemDB
    {
        [Key] public uint m_id { get; set; }
        
        public uint m_roomID { get; set; }
        public byte m_posAnimationFrame { get; set; }

        public uint? m_placedOnFurnitureID { get; set; }
        public uint m_posIdentity { get; set; }
        public byte m_spotOnFurniture { get; set; }
        
        [Required, ForeignKey(nameof(m_roomID))] public virtual NestRoomDB m_room { get; set; }
        [Required, ForeignKey(nameof(m_id))] public virtual NestItemDB m_item { get; set; }
        
        public virtual NestPlacedItemDB? m_placedOnFurniture { get; set; } // ornaments can get placed on furniture
        
        public virtual ICollection<NestPlacedItemDB> m_ornaments { get; set; }
    }
    
    public class NestGardenItemDB 
    {
        [Key] public uint m_id { get; set; }
        [Required] public uint m_nestID { get; set; }
        [Required] public uint m_itemTypeID { get; set; }
        [Required] public ItemColor m_color { get; set; } = new ItemColor();

        [Required, ForeignKey(nameof(m_itemTypeID))] public virtual ItemType m_itemType { get; set; }
        [Required, ForeignKey(nameof(m_nestID))] public virtual NestDB m_nest { get; set; }
        
        public virtual NestPlacedGardenItemDB? m_placedItem { get; set; }
    }
    
    public class NestPlacedGardenItemDB 
    {
        [Key] public uint m_id { get; set; }
        
        public uint m_roomID { get; set; }
        public short m_x { get; set; }
        public short m_z { get; set; }
                
        [Required, ForeignKey(nameof(m_roomID))] public virtual NestRoomDB m_room { get; set; }
        [Required, ForeignKey(nameof(m_id))] public virtual NestGardenItemDB m_item { get; set; }
    }
    
    public class NestSeedItemDB 
    {
        [Key] public uint m_id { get; set; }
        
        [Required] public uint m_seedTypeID { get; set; }
        [Required, ForeignKey(nameof(m_seedTypeID))] public virtual SeedType m_seedType { get; set; }
        
        [Required] public uint m_nestID { get; set; }
        [Required, ForeignKey(nameof(m_nestID))] public virtual NestDB m_nest { get; set; }
        
        public virtual NestPlantDB? m_placedItem { get; set; }
    }
    
    public class NestPlantDB
    {
        [Key] public uint m_id { get; set; }
        
        public short m_x { get; set; }
        public short m_z { get; set; }
        public DateTimeOffset m_growthStartTime { get; set; }
        
        [Required, ForeignKey(nameof(m_id))] public virtual NestSeedItemDB m_item { get; set; }
    }
}