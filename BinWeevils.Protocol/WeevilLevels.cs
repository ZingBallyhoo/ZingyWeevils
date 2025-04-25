namespace BinWeevils.Protocol
{
    public static class WeevilLevels
    {
        private static ReadOnlySpan<int> s_thresholds => 
        [
            0, // level 1
            30,
            60,
            90,
            150,
            300,
            500,
            750,
            1000,
            1400,
            2000,
            2700,
            3600,
            4600,
            5700,
            6900,
            8200,
            9600,
            11000,
            12500,
            14000,
            15600,
            17200,
            18900,
            20600,
            22400,
            24200,
            26100,
            28000,
            30000,
            32000,
            35000,
            38000,
            41000,
            44500,
            48000,
            51500,
            55000,
            59000,
            63000,
            67000,
            71000,
            75000,
            79000,
            84000,
            89000,
            94000,
            99000,
            104000,
            109000,
            115000,
            122000,
            130000,
            139000,
            149000,
            159000,
            169000,
            179000,
            189000,
            200000,
            220000,
            250000,
            290000,
            340000,
            400000,
            460000,
            520000,
            580000,
            640000,
            740000,
            860000,
            1000000,
            1160000,
            1340000,
            1540000,
            1760000,
            2000000,
            2260000,
            2540000,
            2840000
        ];
        
        public static int DetermineLevel(uint xp)
        {
            return DetermineLevel(checked((int)xp));
        }
            
        public static int DetermineLevel(int xp)
        {
            if (xp == 0) return 1;
            
            var index = s_thresholds.BinarySearch(xp);
            if (index >= 0)
            {
                // exactly on the level
                return index+1;
            }
            
            index = ~index;
            return index;
        }
        
        public static void GetLevelThresholds(int level, out int lowerThreshold, out int upperThreshold)
        {
            var levelIdx = level - 1;
            
            lowerThreshold = s_thresholds[levelIdx];
            if (s_thresholds.Length <= levelIdx + 1)
            {
                upperThreshold = -1;
            } else
            {
                upperThreshold = s_thresholds[levelIdx + 1];
            }
        }
        
        public static uint GetXpForLevel(int level)
        {
            return (uint)s_thresholds[level - 1];
        }
    }
}