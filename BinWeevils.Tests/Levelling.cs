using BinWeevils.Protocol;

namespace BinWeevils.Tests;

public class Levelling
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(30, 2)]
    [InlineData(31, 2)]
    [InlineData(2840000, 80)]
    [InlineData(int.MaxValue, 80)]
    public void DetermineLevel(uint xp, int expectedLevel)
    {
        var actualLevel = WeevilLevels.DetermineLevel(xp);
        Assert.Equal(expectedLevel, actualLevel);
    }
}