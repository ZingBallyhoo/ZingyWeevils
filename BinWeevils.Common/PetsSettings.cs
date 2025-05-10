using System.Security.Cryptography;
using CommunityToolkit.HighPerformance;

namespace BinWeevils.Common
{
    public class PetsSettings
    {
        public bool Enabled { get; set; }
        public int MaxNameLength { get; set; }
        public string NameHashSalt { get; set; } = "";
        public uint Cost { get; set; }
        public int MaxUserPets { get; set; }
        public HashSet<uint> Colors { get; set; }
        public HashSet<uint> ItemColors { get; set; }
        public List<uint> BowlItemTypes { get; set; }
        public string BedItem { get; set; }
        public Dictionary<string, PetFoodPack> FoodPacks { get; set; }
        
        public string CalculateNameHash(ReadOnlySpan<char> name)
        {
            var hashBytes = MD5.HashData($"{NameHashSalt}{name}".AsSpan().AsBytes());
            return Convert.ToHexStringLower(hashBytes);
        }
    }
    
    public class PetFoodPack
    {
        public uint Cost { get; set; }
        public uint Feeds { get; set; }
    }
}