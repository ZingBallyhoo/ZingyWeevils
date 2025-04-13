using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Protocol.Sql
{
    [Table("itemType")]
    public class ItemType
    {
        [Key, Column("itemTypeID")] public int m_itemTypeID { get; set; }
        [Column("category")] public ItemCategory m_category { get; set; }
        [Column("configLocation")] public string m_configLocation { get; set; }
        [Column("shopType")] public ItemShopType m_shopType { get; set; }
        [Column("paletteID")] public int m_paletteID { get; set; }
        [Column("defaultHexcolour")] public string m_defaultHexColor { get; set; }
        
        [Column("currency")] public ItemCurrency m_currency { get; set; }
        [Column("price")] public int m_price { get; set; }
        
        [Column("previousCurrency")] public ItemCurrency m_previousCurrency { get; set; }
        [Column("previousPrice")] public int m_previousPrice { get; set; }
        
        [Column("probability")] public int m_probability { get; set; } // todo: type?
        
        [Column("name")] public string m_name { get; set; }
        [Column("description")] public string m_description { get; set; }
        
        [Column("deliveryTime")] public int m_deliveryTime { get; set; }
        [Column("expPoints")] public int m_expPoints { get; set; }
        [Column("powerConsumption")] public int m_powerConsumption { get; set; }
        [Column("boundRadius")] public int m_boundRadius { get; set; }
        [Column("collectionID")] public int? m_collectionID { get; set; }
        [Column("minLevel")] public int m_minLevel { get; set; }
        [Column("tycoonOnly")] public bool m_tycoonOnly { get; set; }
        [Column("canDelete")] public bool m_canDelete { get; set; }
        [Column("canGroup")] public bool m_canGroup { get; set; }
        [Column("isLive")] public bool m_isLive { get; set; }
        [Column("internalCategory")] public ItemInternalCategory m_internalCategory { get; set; }
        [Column("coolness")] public int m_coolness { get; set; }
        [Column("ordering")] public int m_ordering { get; set; }
    }
}