using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinWeevils.Common.Database
{
    public class BlockRecord
    {
        // two users can both block, the block records have no relation to each-other
        [Key] public int m_id { get; set; }
        
        [Required] public int m_forWeevilID { get; set; }
        [Required, ForeignKey(nameof(m_forWeevilID))] public WeevilDB m_forWeevil { get; set; }

        [Required] public int m_blockedWeevilID { get; set; }
        [Required, ForeignKey(nameof(m_blockedWeevilID))] public WeevilDB m_blockedWeevil { get; set; }
    }
}