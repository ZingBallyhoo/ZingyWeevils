using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    [PrimaryKey(nameof(m_weevilID), nameof(m_taskID))]
    public class WeevilTaskRecord
    {
        [Required, Key] public uint m_weevilID { get; set; }
        [Required, ForeignKey(nameof(m_weevilID))] public virtual WeevilDB m_weevil { get; set; }
        
        [Required, Key] public int m_taskID;
    }
    
    public class WeevilCompletedTask : WeevilTaskRecord
    {
    }
    
    public class WeevilRewardedTask : WeevilTaskRecord
    {
    }
}