using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BinWeevils.Common.Database
{
    public class WeevilAccount : IdentityUser
    {
        public uint m_weevilIdx { get; set; }
        [ForeignKey(nameof(m_weevilIdx))] public virtual WeevilDB m_weevil { get; set; }
    }
}