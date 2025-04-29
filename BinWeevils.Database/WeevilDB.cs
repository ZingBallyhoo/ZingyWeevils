using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Sql;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class WeevilDB
    {
        // todo: idx exposes implementation details of the original server...
        // canBuyBinPet = `myLevel >= 8 || myUserIDX <= 1860458`
        
        [Key] public uint m_idx { get; set; }
        [Required] public string m_name { get; set; }
        [Required] public DateTime m_createdAt { get; set; }
        [Required] public DateTime m_lastLogin { get; set; }
        
        [Required] public ulong m_weevilDef { get; set; }
        public uint? m_apparelTypeID { get; set; }
        [Required] public int m_apparelPaletteEntryIndex { get; set; }
        [ForeignKey(nameof(m_apparelTypeID))] public virtual ApparelType? m_apparelType { get; set; }
        
        [Required] public ushort m_introProgress { get; set; }
        
        [Required] public uint m_xp { get; set; }
        [Required] public int m_lastAcknowledgedLevel { get; set; }
        
        [Required] public byte m_food { get; set; }
        [Required] public byte m_fitness { get; set; }
        [Required] public byte m_happiness { get; set; }
        
        [Required] public int m_mulch { get; set; }
        [Required] public int m_dosh { get; set; }
        
        [Required] public virtual NestDB m_nest { get; set; }
        public virtual ICollection<WeevilSpecialMoveDB> m_specialMoves { get; set; }
    }
    
    [PrimaryKey(nameof(m_weevilIdx), nameof(m_action))]
    public class WeevilSpecialMoveDB
    {
        [Key, Required] public uint m_weevilIdx { get; set; }
        [Key, Required] public EWeevilAction m_action { get; set; }
        
        [Required, ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
}