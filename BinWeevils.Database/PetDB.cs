using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BinWeevils.Protocol;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    public class PetDB
    {
        [Key] public uint m_id { get; set; }
        
        public string m_name { get; set; }
        public uint m_bodyColor { get; set; }
        public uint m_antenna1Color { get; set; }
        public uint m_antenna2Color { get; set; }
        public uint m_eye1Color { get; set; }
        public uint m_eye2Color { get; set; }
        
        public byte m_fuel { get; set; }
        public byte m_mentalEnergy { get; set; }
        public byte m_health { get; set; }
        public byte m_fitness { get; set; }
        public uint m_experience { get; set; }
        
        public virtual ICollection<PetSkillDB> m_skills { get; set; }
        
        public virtual NestItemDB m_bedItem { get; set; }
        public virtual NestItemDB m_bowlItem { get; set; }
    }
    
    [PrimaryKey(nameof(m_petID), nameof(m_skillID))]
    public class PetSkillDB
    {
        public uint m_petID { get; set; }
        public EPetSkill m_skillID { get; set; }
        
        public byte m_obedience { get; set; } = 20;
        public double m_skillLevel { get; set; } = 0;
        
        [ForeignKey(nameof(m_petID))] public PetDB m_pet { get; set; }
    }
}