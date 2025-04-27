using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    [PrimaryKey(nameof(m_paletteID), nameof(m_index))]
    public class PaletteEntryDB
    {
        [Required, Key] public uint m_paletteID;
        [Required, Key] public int m_index;
        
        [Required] public string m_color;
    }
}