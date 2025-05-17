using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Sql;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Common.Database
{
    public class WeevilDB
    {
        // todo: idx exposes implementation details of the original server...
        // canBuyBinPet = `myLevel >= 8 || myUserIDX <= 1860458`
        
        [Key] public uint m_idx { get; set; }
        public string m_name { get; set; }
        public DateTime m_createdAt { get; set; }
        public DateTime m_lastLogin { get; set; }
        
        public ulong m_weevilDef { get; set; }
        public uint? m_apparelTypeID { get; set; }
        public int m_apparelPaletteEntryIndex { get; set; }
        [ForeignKey(nameof(m_apparelTypeID))] public virtual ApparelType? m_apparelType { get; set; }
        
        public ushort m_introProgress { get; set; }
        
        public uint m_xp { get; set; }
        public int m_lastAcknowledgedLevel { get; set; }
        
        public byte m_food { get; set; }
        public byte m_fitness { get; set; }
        public byte m_happiness { get; set; }
        
        public int m_mulch { get; set; }
        public int m_dosh { get; set; }
        
        public uint m_petFoodStock { get; set; } = 15;
        
        public virtual NestDB m_nest { get; set; }
        public virtual ICollection<IgnoreRecordDB> m_ignoredWeevils { get; set; }
        public virtual ICollection<WeevilSpecialMoveDB> m_specialMoves { get; set; }
        public virtual ICollection<PetDB> m_pets { get; set; }
        public virtual ICollection<WeevilGamePlayedDB> m_gamesPlayed { get; set; }
        public virtual ICollection<WeevilTurnBasedGamePlayedDB> m_turnBasedGamesPlayed { get; set; }
        public virtual ICollection<WeevilTrackPersonalBestDB> m_trackPBs { get; set; }
    }
    
    [PrimaryKey(nameof(m_weevilIdx), nameof(m_action))]
    public class WeevilSpecialMoveDB
    {
        [Key] public uint m_weevilIdx { get; set; }
        [Key] public EWeevilAction m_action { get; set; }
        
        [ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
    
    [PrimaryKey(nameof(m_weevilIdx), nameof(m_gameType))]
    public class WeevilGamePlayedDB
    {
        [Key] public uint m_weevilIdx { get; set; }
        [Key] public EGameType m_gameType { get; set; }
        
        public DateTime m_lastPlayed { get; set; }
        
        [ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
    
    [PrimaryKey(nameof(m_weevilIdx), nameof(m_gameType))]
    public class WeevilTurnBasedGamePlayedDB
    {
        [Key] public uint m_weevilIdx { get; set; }
        [Key] public ETurnBasedGameType m_gameType { get; set; }
        
        public DateTime m_lastPlayed { get; set; }
        
        [ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
    
    [PrimaryKey(nameof(m_weevilIdx), nameof(m_gameType))]
    public class WeevilTrackPersonalBestDB
    {
        [Key] public uint m_weevilIdx { get; set; }
        [Key] public EGameType m_gameType { get; set; }
        
        public double m_lap1 { get; set; }
        public double m_lap2 { get; set; }
        public double m_lap3 { get; set; }
        
        [ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
        
        public double m_total => m_lap1 + m_lap2 + m_lap3;
    }
}