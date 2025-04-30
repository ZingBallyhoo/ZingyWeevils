using System.ComponentModel.DataAnnotations;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Database
{
    [PrimaryKey(nameof(m_paletteID), nameof(m_index))]
    public class PaletteEntryDB
    {
        [Required, Key] public uint m_paletteID { get; set; }
        [Required, Key] public int m_index { get; set; }
        
        [Required] public string m_colorString { get; set; } // todo: could be removed?
        [Required] public ItemColor m_color { get; set; }
    }
}