using System.ComponentModel.DataAnnotations;

namespace BinWeevils.Database
{
    public class WeevilDB
    {
        [Key] public int m_idx { get; set; }
        [Required] public string m_name { get; set; }
        [Required] public ulong m_weevilDef { get; set; }
        
        [Required] public uint m_xp { get; set; }
        [Required] public int m_lastAcknowledgedLevel { get; set; }
        [Required] public byte m_food { get; set; }
        [Required] public byte m_fitness { get; set; }
        [Required] public byte m_happiness { get; set; }
        
        [Required] public virtual WalletDB m_wallet { get; set; }
    }
}