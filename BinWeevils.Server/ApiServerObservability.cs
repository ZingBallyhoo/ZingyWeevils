using System.Diagnostics;
using System.Diagnostics.Metrics;
using BinWeevils.Protocol.Sql;

namespace BinWeevils.Server
{
    public static class ApiServerObservability
    {
        public static readonly ActivitySource s_source = new ActivitySource("BinWeevils.Server");
        public static readonly Meter s_meter = new Meter("BinWeevils.Server");
        
        public static readonly Counter<int> s_connectionAttempts = s_meter.CreateCounter<int>("bw_connection_attempts");
        public static readonly UpDownCounter<int> s_activeConnections = s_meter.CreateUpDownCounter<int>("bw_active_connections");
        
        public static readonly Counter<int> s_nestItemsBought = s_meter.CreateCounter<int>("bw_nest_items_bought");
        public static readonly Counter<int> s_gardenItemsBought = s_meter.CreateCounter<int>("bw_garden_items_bought");
        public static readonly Counter<int> s_seedsBought = s_meter.CreateCounter<int>("bw_seeds_bought");
        public static readonly Counter<int> s_haggleItemsSold = s_meter.CreateCounter<int>("bw_haggle_items_sold");
        public static readonly Counter<double> s_haggleTotalPayout = s_meter.CreateCounter<double>("bw_haggle_total_payout");
        
        public static readonly Counter<int> s_buddyMessagesSent = s_meter.CreateCounter<int>("bw_buddy_messages_sent");
        public static readonly Counter<int> s_buddyMessagesRead = s_meter.CreateCounter<int>("bw_buddy_messages_read");
        public static readonly Counter<int> s_buddyMessagesDeleted = s_meter.CreateCounter<int>("bw_buddy_messages_deleted");
        public static readonly Counter<int> s_buddyMessagesDeletedSys = s_meter.CreateCounter<int>("bw_buddy_messages_deleted_sys");
        
        public static readonly Counter<int> s_businessLocationsBought = s_meter.CreateCounter<int>("bw_business_locations_bought");
        public static readonly Counter<int> s_nestRoomsBought = s_meter.CreateCounter<int>("bw_nest_rooms_bought");
        public static readonly Counter<double> s_nestFuelBought = s_meter.CreateCounter<double>("bw_nest_fuel_bought");
        public static readonly Counter<int> s_nestItemsPlaced = s_meter.CreateCounter<int>("bw_nest_items_placed");
        public static readonly Counter<int> s_nestItemsMoved = s_meter.CreateCounter<int>("bw_nest_items_moved");
        public static readonly Counter<int> s_nestItemsRemoved = s_meter.CreateCounter<int>("bw_nest_items_removed");
        public static readonly Counter<int> s_levelUpsProcessed = s_meter.CreateCounter<int>("bw_level_ups_processed");
        
        public static readonly Counter<int> s_gardenItemsPlaced = s_meter.CreateCounter<int>("bw_garden_items_placed");
        public static readonly Counter<int> s_gardenItemsMoved = s_meter.CreateCounter<int>("bw_garden_items_moved");
        public static readonly Counter<int> s_gardenItemsRemoved = s_meter.CreateCounter<int>("bw_garden_items_removed");
        public static readonly Counter<int> s_gardenPlantsPlaced = s_meter.CreateCounter<int>("bw_garden_plants_placed");
        public static readonly Counter<int> s_gardenPlantsMoved = s_meter.CreateCounter<int>("bw_garden_plants_moved");
        public static readonly Counter<int> s_gardenPlantsWatered = s_meter.CreateCounter<int>("bw_garden_plants_watered");
        public static readonly Counter<int> s_gardenPlantsRemoved = s_meter.CreateCounter<int>("bw_garden_plants_removed");
        public static readonly Counter<int> s_gardenUpgradesBought = s_meter.CreateCounter<int>("bw_garden_upgrades_bought");
        
        public static readonly Counter<int> s_gamesPlayedGivingRewards = s_meter.CreateCounter<int>("bw_games_played_giving_rewards");
        public static readonly Counter<int> s_gamesPlayedTotal = s_meter.CreateCounter<int>("bw_games_played_total");
        
        public static Activity? StartActivity(string name)
        {
            return s_source.StartActivity(name);
        }
        
        private static KeyValuePair<string, object?> GetSeedCategoryTag(SeedCategory category)
        {
            return new KeyValuePair<string,object?>("category", category);
        }
        
        public static void AddPlantPlaced(SeedCategory category)
        {
            s_gardenPlantsPlaced.Add(1, GetSeedCategoryTag(category));
        }
        
        public static void AddPlantHarvest(SeedCategory category)
        {
            s_gardenPlantsWatered.Add(1, GetSeedCategoryTag(category));
        }
        
        public static void AddPlantRemoved(SeedCategory category)
        {
            s_gardenPlantsRemoved.Add(1, GetSeedCategoryTag(category));
        }
    }
}