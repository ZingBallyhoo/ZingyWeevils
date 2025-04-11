using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Database
{
    public class WalletDB
    {
        [Required, Key] public int m_weevilIdx { get; set; }
        [Required, ForeignKey(nameof(m_weevilIdx))] public WeevilDB m_weevil { get; set; }
        
        [Required, ConcurrencyCheck] public int m_mulch { get; set; }
        [Required, ConcurrencyCheck] public int m_dosh { get; set; }
    }
}