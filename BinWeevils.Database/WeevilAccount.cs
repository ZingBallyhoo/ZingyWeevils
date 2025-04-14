using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BinWeevils.Database
{
    public class WeevilAccount : IdentityUser
    {
        [Required] public uint m_weevilIdx { get; set; }
        [Required, ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
}