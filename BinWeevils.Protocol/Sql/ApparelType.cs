using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Protocol.Sql
{
    [Table("apparelTypes")]
    public class ApparelType
    {
        [Key, Column("id")] public uint m_id { get; set; }
        [Column("category")] public byte m_category { get; set; }
        [Column("name")] public string m_name { get; set; }
        [Column("description")] public string m_description { get; set; }
        [Column("paletteId")] public uint m_paletteID { get; set; }
        [Column("price")] public uint m_price { get; set; }
        [Column("probability")] public byte m_probability { get; set; }
        [Column("minLevel")] public byte m_minLevel { get; set; }
        [Column("tycoonOnly")] public bool m_tycoonOnly { get; set; }
    }
}