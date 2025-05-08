using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Common.Database
{
    [PrimaryKey(nameof(m_weevilID), nameof(m_taskID))]
    public class WeevilTaskRecord
    {
        [Required, Key] public uint m_weevilID { get; set; }
        [Required, ForeignKey(nameof(m_weevilID))] public virtual WeevilDB m_weevil { get; set; }
        
        [Required, Key] public int m_taskID { get; set; }
    }
    
    public class CompletedTaskDB : WeevilTaskRecord
    {
    }
    
    public class RewardedTaskDB : WeevilTaskRecord
    {
    }
}