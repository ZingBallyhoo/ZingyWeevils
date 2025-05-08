namespace BinWeevils.Common
{
    public class PetsSettings
    {
        public bool Enabled { get; set; }
        public int MaxNameLength { get; set; }
        public string NameHashSalt { get; set; } = "";
        public int Cost { get; set; }
        public int MaxUserPets { get; set; }
        public HashSet<uint> Colors { get; set; }
        public HashSet<uint> ItemColors { get; set; }
        public List<uint> BowlItemTypes { get; set; }
        public string BedItem { get; set; }
    }
}