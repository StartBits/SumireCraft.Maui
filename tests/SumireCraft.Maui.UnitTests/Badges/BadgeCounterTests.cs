using System.ComponentModel;
using System.Globalization;
using SumireCraft.Maui.Badges;

namespace SumireCraft.Maui.UnitTests.Badges;

public class BadgeCounterTests
{
    [Fact]
    public void TheDefaultRules_CapAt99AndHideZero()
    {
        var counter = new BadgeCounter();

        Assert.Equal(99, counter.MaxCount);
        Assert.False(counter.ShowZero);
    }

    [Fact]
    public void ANewCounter_IsHiddenWithEmptyText()
    {
        var counter = new BadgeCounter();

        Assert.Equal(0, counter.Count);
        Assert.False(counter.IsVisible);
        Assert.Equal("", counter.Text);
    }

    /// <remarks>
    /// German groups thousands with a full stop, so a count written the culture's way would read
    /// "1.000". The culture belongs to this test's own flow, so no other test sees it.
    /// </remarks>
    [Fact]
    public void TheText_IsWrittenTheSameWayWhateverTheCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        try
        {
            var counter = new BadgeCounter(maxCount: 9999) { Count = 1000 };

            Assert.Equal("1000", counter.Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(5, "5")]
    [InlineData(99, "99")]
    public void APositiveCount_IsShownAsItIs(int count, string expected)
    {
        var counter = new BadgeCounter { Count = count };

        Assert.True(counter.IsVisible);
        Assert.Equal(expected, counter.Text);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void ACountAboveTheMaximum_IsShownAsTheMaximumPlus(int count)
    {
        var counter = new BadgeCounter { Count = count };

        Assert.True(counter.IsVisible);
        Assert.Equal("99+", counter.Text);
    }

    [Theory]
    [InlineData(9, "9")]
    [InlineData(10, "9+")]
    [InlineData(100, "9+")]
    public void TheMaximum_CanBeChanged(int count, string expected)
    {
        var counter = new BadgeCounter(maxCount: 9) { Count = count };

        Assert.Equal(9, counter.MaxCount);
        Assert.Equal(expected, counter.Text);
    }

    [Fact]
    public void ALargeMaximum_ShowsTheNumberWithoutSeparators()
    {
        var counter = new BadgeCounter(maxCount: 9999) { Count = 1234 };

        Assert.Equal("1234", counter.Text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AMaximumBelowOne_IsRejected(int maxCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BadgeCounter(maxCount));
    }

    [Fact]
    public void ShowZero_ShowsANewCounterAsZero()
    {
        var counter = new BadgeCounter(showZero: true);

        Assert.True(counter.ShowZero);
        Assert.True(counter.IsVisible);
        Assert.Equal("0", counter.Text);
    }

    [Fact]
    public void ShowZero_LeavesTheOtherRulesAlone()
    {
        var counter = new BadgeCounter(showZero: true) { Count = 100 };

        Assert.True(counter.IsVisible);
        Assert.Equal("99+", counter.Text);
    }

    [Fact]
    public void ANegativeCount_IsTreatedAsZero()
    {
        var counter = new BadgeCounter { Count = 5 };

        counter.Count = -3;

        Assert.Equal(0, counter.Count);
        Assert.False(counter.IsVisible);
        Assert.Equal("", counter.Text);
    }

    [Fact]
    public void ANegativeCount_IsShownAsZeroUnderShowZero()
    {
        var counter = new BadgeCounter(showZero: true) { Count = -3 };

        Assert.True(counter.IsVisible);
        Assert.Equal("0", counter.Text);
    }

    [Fact]
    public void GoingFromZeroToACount_AnnouncesEverything()
    {
        var counter = new BadgeCounter();
        var raised = Record(counter);

        counter.Count = 5;

        Assert.Equal(
            [nameof(BadgeCounter.Count), nameof(BadgeCounter.IsVisible), nameof(BadgeCounter.Text)],
            raised);
    }

    [Fact]
    public void GoingBackToZero_AnnouncesEverything()
    {
        var counter = new BadgeCounter { Count = 5 };
        var raised = Record(counter);

        counter.Count = 0;

        Assert.Equal(
            [nameof(BadgeCounter.Count), nameof(BadgeCounter.IsVisible), nameof(BadgeCounter.Text)],
            raised);
    }

    [Fact]
    public void ChangingOneCountToAnother_LeavesIsVisibleQuiet()
    {
        var counter = new BadgeCounter { Count = 3 };
        var raised = Record(counter);

        counter.Count = 5;

        Assert.Equal([nameof(BadgeCounter.Count), nameof(BadgeCounter.Text)], raised);
    }

    [Fact]
    public void ChangingACountThatStaysAboveTheMaximum_AnnouncesOnlyTheCount()
    {
        var counter = new BadgeCounter { Count = 100 };
        var raised = Record(counter);

        counter.Count = 200;

        Assert.Equal([nameof(BadgeCounter.Count)], raised);
    }

    [Fact]
    public void GoingFromZeroToACountUnderShowZero_LeavesIsVisibleQuiet()
    {
        var counter = new BadgeCounter(showZero: true);
        var raised = Record(counter);

        counter.Count = 5;

        Assert.Equal([nameof(BadgeCounter.Count), nameof(BadgeCounter.Text)], raised);
    }

    [Fact]
    public void SettingTheSameCount_AnnouncesNothing()
    {
        var counter = new BadgeCounter { Count = 5 };
        var raised = Record(counter);

        counter.Count = 5;

        Assert.Empty(raised);
    }

    [Fact]
    public void ANegativeCountAtZero_AnnouncesNothing()
    {
        var counter = new BadgeCounter();
        var raised = Record(counter);

        counter.Count = -1;

        Assert.Empty(raised);
    }

    [Fact]
    public void TheSenderIsTheCounter()
    {
        var counter = new BadgeCounter();
        object? sender = null;
        counter.PropertyChanged += (s, _) => sender = s;

        counter.Count = 1;

        Assert.Same(counter, sender);
    }

    private static List<string?> Record(INotifyPropertyChanged source)
    {
        var raised = new List<string?>();
        source.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        return raised;
    }
}
