using BinWeevils.Protocol.Sql;

namespace BinWeevils.Common
{
    public class EconomySettings
    {
        public float ShopXpScalar { get; set; } = 10;
        public float ShopDoshToMulch { get; set; } = 500;
        
        public uint MaxMulchPerGame { get; set; } = 5000;
        public uint MaxXpPerGame { get; set; } = 2000;
        public float GameScoreToMulch { get; set; } = 3;
        public float GameScoreToXp { get; set; } = 0.25f;
        
        public uint DailyBrainMaxScore { get; set; } = 500; // not really but using to scale
        public uint DailyBrainMaxMulch { get; set; } = 5000;
        public uint DailyBrainMaxXp { get; set; } = 1000;
        
        public bool InstantPlants { get; set; } = true;
        public float PlantMulchScalar { get; set; } = 5;
        public float PlantXpScalar { get; set; } = 20;
        
        public uint GetItemXp(int originalXp)
        {
            return (uint)(originalXp * ShopXpScalar);
        }
        
        public uint GetItemCost(int originalCost, ItemCurrency currency)
        {
            return currency switch
            {
                ItemCurrency.Dosh => (uint)(originalCost * ShopDoshToMulch),
                ItemCurrency.None => throw new InvalidDataException("item doesn't have a currency"),
                _ => (uint)originalCost,
            };
        }
        
        public uint GetPlantMulchYield(uint mulchYield)
        {
            return (uint)(mulchYield * PlantMulchScalar);
        }
        
        public uint GetPlantXpYield(uint xpYield)
        {
            return (uint)(xpYield * PlantXpScalar);
        }
        
        public uint GetPlantGrowTime(uint growTime)
        {
            if (InstantPlants)
            {
                return 2;
            }
            return growTime;
        }
        
        public uint GetPlantCycleTime(uint cycleTime, SeedCategory category)
        {
            if (category == SeedCategory.Perishable) return cycleTime; // don't perish instantly
            
            if (InstantPlants)
            {
                return 2;
            }
            return cycleTime;
        }
    }
}