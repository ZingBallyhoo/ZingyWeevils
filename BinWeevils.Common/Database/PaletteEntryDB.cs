using System.ComponentModel.DataAnnotations;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Common.Database
{
    [PrimaryKey(nameof(m_paletteID), nameof(m_index))]
    public class PaletteEntryDB
    {
        [Key] public uint m_paletteID { get; set; }
        [Key] public int m_index { get; set; }
        
        public string m_colorString { get; set; } // todo: could be removed?
        public ItemColor m_color { get; set; }
    }
}