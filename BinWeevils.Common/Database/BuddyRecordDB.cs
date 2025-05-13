using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Common.Database
{
    [PrimaryKey(nameof(m_weevil1ID), nameof(m_weevil2ID))]
    public class BuddyRecordDB
    {
        [Key] public uint m_weevil1ID { get; set; }
        [Key] public uint m_weevil2ID { get; set; }
        
        [ForeignKey(nameof(m_weevil1ID))] public WeevilDB m_weevil1 { get; set; }
        [ForeignKey(nameof(m_weevil2ID))] public WeevilDB m_weevil2 { get; set; }
    }
}