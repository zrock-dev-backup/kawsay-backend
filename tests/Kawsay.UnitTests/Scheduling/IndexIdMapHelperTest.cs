using Api.Scheduling.Utils;

namespace Kawsay.UnitTests.Scheduling;

public class IndexIdMapHelperTest
{
    // invalid output is even sorting ids in ascending order they are not locally sorted i.e. monday to friday or
    // 8:30 to 21:00
    [Fact]
    public void GetId_WithIndexInput_ReturnsValidValuePair()
    {
        var amntDays = 1;
        var amntPeriods = 5;
        var data = new Dictionary<int, List<int>>
        {
            { 100, new List<int> { 500, 501, 502, 503, 504 } },
        };
        var yuleMapperHelper = new IndexIdMapHelper(amntDays, amntPeriods, data);

        var dayId = 100;
        var periodId = 504;
        var dayIndex = 0;
        var periodIndex = 4;
        Assert.Equal(new Pair(dayId, periodId), yuleMapperHelper.GetId(new Pair(dayIndex, periodIndex)));
    }

    [Fact]
    public void GetId_WithIdInput_ReturnsValidValuePair()
    {
        var amntDays = 1;
        var amntPeriods = 5;
        var data = new Dictionary<int, List<int>>
        {
            { 100, new List<int> { 500, 501, 502, 503, 504 } },
        };
        var yuleMapperHelper = new IndexIdMapHelper(amntDays, amntPeriods, data);

        var dayId = 100;
        var periodId = 504;
        var dayIndex = 0;
        var periodIndex = 4;
        Assert.Equal(new Pair(dayIndex, periodIndex), yuleMapperHelper.GetId(new Pair(dayId, periodId)));
    }
}