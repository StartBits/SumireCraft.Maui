using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// The shape Android and iOS draw a border as: the outline of the input, less the same outline taken
// in by the thickness of each side. Windows draws the border itself.
public class BorderGeometryTests
{
    [Fact]
    public void TheInside_IsTheOutlineTakenInByEachSide()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 100, 40), new Thickness(1, 2, 3, 4), new CornerRadius(0), 1);

        Assert.Equal(new RectF(1, 2, 96, 34), inside.Bounds);
    }

    [Fact]
    public void TheInsidesCorners_AreRoundedLessByTheThickerOfTheirSides()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 100, 40), new Thickness(1, 2, 3, 4), new CornerRadius(10), 1);

        Assert.Equal(new CornerRadius(8, 7, 6, 6), inside.CornerRadius);
    }

    [Fact]
    public void AThickTop_RoundsBothTopCornersLess()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 100, 40), new Thickness(1, 4, 1, 1), new CornerRadius(10), 1);

        Assert.Equal(new CornerRadius(6, 6, 9, 9), inside.CornerRadius);
    }

    [Fact]
    public void ACornerRoundedLessThanItsSides_IsSquareInside()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 100, 40), new Thickness(4), new CornerRadius(2), 1);

        Assert.Equal(new CornerRadius(0), inside.CornerRadius);
    }

    [Fact]
    public void SidesThickerThanTheInput_LeaveNoInside()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 10, 10), new Thickness(8), new CornerRadius(0), 1);

        Assert.Equal(0, inside.Bounds.Width);
        Assert.Equal(0, inside.Bounds.Height);
    }

    // Android draws in pixels, so a thickness and a radius in device-independent units are scaled by
    // the screen's density; the bounds are pixels already.
    [Fact]
    public void ThicknessAndRadius_AreScaledByTheDensity()
    {
        var inside = BorderGeometry.Inside(new RectF(0, 0, 300, 120), new Thickness(0, 0, 0, 1), new CornerRadius(4), 3);

        Assert.Equal(new RectF(0, 0, 300, 117), inside.Bounds);
        Assert.Equal(new CornerRadius(12, 12, 9, 9), inside.CornerRadius);
    }

    [Fact]
    public void TheOutline_IsScaledByTheDensity()
    {
        var outline = BorderGeometry.Outline(new RectF(0, 0, 300, 120), new CornerRadius(4, 0, 0, 2), 3);

        Assert.Equal(new RectF(0, 0, 300, 120), outline.Bounds);
        Assert.Equal(new CornerRadius(12, 0, 0, 6), outline.CornerRadius);
    }
}
