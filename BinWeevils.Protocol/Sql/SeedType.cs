using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Protocol.Sql
{
    [Table("seeds")]
    public class SeedType
    {
        [Key, Column("id")] public uint m_id { get; set; }
        [Column("category")] public SeedCategory m_category { get; set; }
        [Column("tycoon")] public bool m_tycoon { get; set; }
        [Column("level")] public int m_level { get; set; }
        [Column("fileName")] public string m_fileName { get; set; }
        [Column("name")] public string m_name { get; set; }
        [Column("price")] public int m_price { get; set; }
        [Column("mulchYield")] public int mulchYield { get; set; }
        [Column("xpYield")] public int xpYield { get; set; }
        [Column("growTime")] public int growTime { get; set; }
        [Column("cycleTime")] public int cycleTime { get; set; }
        [Column("probability")] public int probability { get; set; }
        [Column("radius")] public int radius { get; set; }
    }
    
    public enum SeedCategory
    {
        Perishable = 1,
        Reharvest = 2,
    }
}